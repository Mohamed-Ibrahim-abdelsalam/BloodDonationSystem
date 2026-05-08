using AutoMapper;
using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using DomainLayer.Interfaces;
using DomainLayer.Specifications;
using Microsoft.AspNetCore.Identity;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        // Threshold for "Active" status — donated within last 3 months
        private static readonly TimeSpan ActiveThreshold = TimeSpan.FromDays(90);

        public AdminService(
            IUnitOfWork uow,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET /api/admin/requests
        // ══════════════════════════════════════════════════════════════════════

        public async Task<AdminRequestsDashboardDto> GetRequestsDashboardAsync(
            string userId,
            PaginationParams pagination)
        {
            var (user, role) = await ResolveUserAndRoleAsync(userId);

            bool isAppAdmin = role == Role.AppAdmin;
            int? hospitalId = isAppAdmin ? null : user.HospitalId;

            if (!isAppAdmin && hospitalId is null)
                throw new InvalidOperationException(
                    "Hospital admin is not linked to any hospital.");

            // STEP 1: statistics — 4 lightweight COUNT queries
            var statistics = await BuildRequestStatisticsAsync(hospitalId);

            // STEP 2: current page data
            var dataSpec = isAppAdmin
                ? (ISpecification<BloodRequest>)new AllRequestsPagedSpecification(
                      pagination.PageNumber, pagination.PageSize)
                : new HospitalRequestsPagedSpecification(
                      hospitalId!.Value, pagination.PageNumber, pagination.PageSize);

            var pagedRequests = await _uow.BloodRequests.GetAllWithSpecAsync(dataSpec);
            var items = _mapper.Map<IEnumerable<AdminRequestItemDto>>(pagedRequests);

            return new AdminRequestsDashboardDto
            {
                Statistics = statistics,
                Requests = PaginatedResponse<AdminRequestItemDto>.Create(
                    data: items,
                    totalCount: statistics.TotalRequests,
                    pageNumber: pagination.PageNumber,
                    pageSize: pagination.PageSize),
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET /api/admin/donations
        // ══════════════════════════════════════════════════════════════════════

        public async Task<AdminDonationsDashboardDto> GetDonationsDashboardAsync(
            string userId,
            PaginationParams pagination)
        {
            var (user, role) = await ResolveUserAndRoleAsync(userId);

            bool isAppAdmin = role == Role.AppAdmin;
            int? hospitalId = isAppAdmin ? null : user.HospitalId;

            if (!isAppAdmin && hospitalId is null)
                throw new InvalidOperationException(
                    "Hospital admin is not linked to any hospital.");

            // STEP 1: statistics
            var statistics = await BuildDonationStatisticsAsync(hospitalId);

            // STEP 2: current page data
            var dataSpec = isAppAdmin
                ? (ISpecification<Donation>)new AllDonationsPagedSpecification(
                      pagination.PageNumber, pagination.PageSize)
                : new HospitalDonationsPagedSpecification(
                      hospitalId!.Value, pagination.PageNumber, pagination.PageSize);

            var pagedDonations = await _uow.Donations.GetAllWithSpecAsync(dataSpec);
            var items = _mapper.Map<IEnumerable<AdminDonationItemDto>>(pagedDonations);

            return new AdminDonationsDashboardDto
            {
                Statistics = statistics,
                Donations = PaginatedResponse<AdminDonationItemDto>.Create(
                    data: items,
                    totalCount: statistics.TotalDonations,
                    pageNumber: pagination.PageNumber,
                    pageSize: pagination.PageSize),
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET /api/admin/users
        // ══════════════════════════════════════════════════════════════════════

        public async Task<PaginatedResponse<AdminUserListItemDto>> GetUsersAsync(
            string userId,
            PaginationParams pagination)
        {
            var (admin, role) = await ResolveUserAndRoleAsync(userId);

            if (role == Role.AppAdmin)
                return await GetAllUsersPaginatedAsync(pagination);

            // HospitalAdmin path
            if (admin.HospitalId is null)
                throw new InvalidOperationException(
                    "Hospital admin is not linked to any hospital.");

            return await GetHospitalDonorsPaginatedAsync(admin.HospitalId.Value, pagination);
        }

        // ── App Admin: all users with Role == User ────────────────────────────
        private async Task<PaginatedResponse<AdminUserListItemDto>> GetAllUsersPaginatedAsync(
            PaginationParams pagination)
        {
            // COUNT — simple SELECT COUNT(*) WHERE Role = 'User'
            var countSpec = new AllUsersCountSpecification();
            var totalCount = await _uow.Users.CountAsync(countSpec);

            // DATA — ordered by computed LastDonationDate (latest confirmed donation)
            // We cannot ORDER BY a sub-query max date in the Specification pattern
            // without raw SQL, so we fetch the page with all donations included
            // then let EF materialise the objects. Ordering by a navigation-aggregate
            // needs the full Users+Donations data.
            //
            // Strategy: fetch ALL users with Role==User (with Donations included),
            // sort in memory, then apply manual pagination.
            // This is acceptable because Role==User rows are the donor base;
            // for very large datasets a raw SQL approach should be considered.
            var allSpec = new AllUsersWithDonationsSpecification();
            var allUsers = await _uow.Users.GetAllWithSpecAsync(allSpec);

            // Sort by most recent confirmed donation DESC; users with no donations last
            var ordered = allUsers
                .OrderByDescending(u => u.Donations
                    .Where(d => d.Status == DonationStatus.Confirmed)
                    .Select(d => d.ConfirmedAt)
                    .Max())   // null for users with no confirmed donations
                .ToList();

            // Manual pagination on the sorted in-memory list
            var page = ordered
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var items = page.Select(u => ToUserListItem(u)).ToList();

            return PaginatedResponse<AdminUserListItemDto>.Create(
                data: items,
                totalCount: totalCount,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize);
        }

        // ── Hospital Admin: unique donors with confirmed donations ─────────────
        private async Task<PaginatedResponse<AdminUserListItemDto>> GetHospitalDonorsPaginatedAsync(
            int hospitalId,
            PaginationParams pagination)
        {
            // STEP 1: get all confirmed donations for this hospital
            // to discover unique donor IDs and their donation history
            var donationsSpec = new HospitalConfirmedDonationsSpecification(hospitalId);
            var allDonations = await _uow.Donations.GetAllWithSpecAsync(donationsSpec);

            // STEP 2: group by donor, keep only unique users,
            // sort by their most recent confirmed donation DESC
            var donorGroups = allDonations
                .GroupBy(d => d.DonorUserId)
                .Select(g => new
                {
                    DonorId = g.Key,
                    LastDonationAt = g.Max(d => d.ConfirmedAt),
                })
                .OrderByDescending(g => g.LastDonationAt)
                .ToList();

            var totalCount = donorGroups.Count;

            // STEP 3: paginate the donor ID list
            var pagedDonorIds = donorGroups
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(g => g.DonorId)
                .ToList();

            if (!pagedDonorIds.Any())
                return PaginatedResponse<AdminUserListItemDto>.Create(
                    data: Enumerable.Empty<AdminUserListItemDto>(),
                    totalCount: totalCount,
                    pageNumber: pagination.PageNumber,
                    pageSize: pagination.PageSize);

            // STEP 4: fetch full user profiles for the page (includes Donations)
            var usersSpec = new HospitalDonorDonationsSpecification(hospitalId, pagedDonorIds);
            var donations = await _uow.Donations.GetAllWithSpecAsync(usersSpec);

            // Project donations → unique users preserving the sort order
            var userMap = donations
                .GroupBy(d => d.DonorUserId)
                .ToDictionary(g => g.Key, g => g.First().DonorUser);

            // Build items in the ORDER of pagedDonorIds (sorted list)
            var items = pagedDonorIds
                .Where(id => userMap.ContainsKey(id))
                .Select(id =>
                {
                    var user = userMap[id];
                    // For Hospital Admin view: LastDonation = latest confirmed at THIS hospital
                    var lastDonationAtHospital = donations
                        .Where(d => d.DonorUserId == id)
                        .Max(d => d.ConfirmedAt);

                    return ToUserListItem(user, lastDonationAtHospital);
                })
                .ToList();

            return PaginatedResponse<AdminUserListItemDto>.Create(
                data: items,
                totalCount: totalCount,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GET /api/admin/users/{id}  — App Admin only
        // ══════════════════════════════════════════════════════════════════════

        public async Task<AdminUserDetailDto> GetUserByIdAsync(
            string requestingUserId,
            string targetUserId)
        {
            // Verify the requesting user is an App Admin
            var (admin, role) = await ResolveUserAndRoleAsync(requestingUserId);

            if (role != Role.AppAdmin)
                throw new UnauthorizedAccessException(
                    "Access denied. Only AppAdmin can view user details.");

            // Fetch target user with their donations
            var spec = new UserByIdWithDonationsSpecification(targetUserId);
            var user = await _uow.Users.GetEntityWithSpecAsync(spec)
                ?? throw new KeyNotFoundException($"User with id '{targetUserId}' was not found.");

            // Map flat fields via AutoMapper
            var dto = _mapper.Map<AdminUserDetailDto>(user);

            // Compute derived fields
            var lastDonation = user.Donations
                .Where(d => d.Status == DonationStatus.Confirmed)
                .Select(d => d.ConfirmedAt)
                .Max();

            dto.LastDonation = lastDonation;
            dto.Status = ComputeStatus(lastDonation);

            return dto;
        }

        // ══════════════════════════════════════════════════════════════════════
        // Private helpers
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Converts an ApplicationUser + optional override LastDonation date into
        /// an AdminUserListItemDto, computing Status from LastDonation.
        /// When lastDonationOverride is null, derives it from user.Donations.
        /// </summary>
        private AdminUserListItemDto ToUserListItem(
            ApplicationUser user,
            DateTime? lastDonationOverride = null)
        {
            var dto = _mapper.Map<AdminUserListItemDto>(user);

            var lastDonation = lastDonationOverride
                ?? user.Donations
                       .Where(d => d.Status == DonationStatus.Confirmed)
                       .Select(d => d.ConfirmedAt)
                       .Max();

            dto.LastDonation = lastDonation;
            dto.Status = ComputeStatus(lastDonation);

            return dto;
        }

        /// <summary>
        /// Returns "Active" if lastDonation is within the last 3 months, else "Inactive".
        /// Users who have never donated are always "Inactive".
        /// </summary>
        private static string ComputeStatus(DateTime? lastDonation)
        {
            if (lastDonation is null)
                return "Inactive";

            return (DateTime.UtcNow - lastDonation.Value) <= ActiveThreshold
                ? "Active"
                : "Inactive";
        }

        // ── Request statistics ────────────────────────────────────────────────

        private async Task<RequestStatisticsDto> BuildRequestStatisticsAsync(int? hospitalId)
        {
            ISpecification<BloodRequest> MakeSpec(BloodRequestStatus? status) =>
                (hospitalId, status) switch
                {
                    (null, null) => new AllRequestsCountSpecification(),
                    (null, var s) => new RequestsByStatusCountSpec(s!.Value),
                    (var h, null) => new HospitalRequestsCountSpecification(h!.Value),
                    (var h, var s) => new RequestsByStatusAndHospitalCountSpec(s!.Value, h!.Value),
                };

            var total = await _uow.BloodRequests.CountAsync(MakeSpec(null));
            var open = await _uow.BloodRequests.CountAsync(MakeSpec(BloodRequestStatus.Open));
            var fulfilled = await _uow.BloodRequests.CountAsync(MakeSpec(BloodRequestStatus.Fulfilled));
            var completed = await _uow.BloodRequests.CountAsync(MakeSpec(BloodRequestStatus.Completed));

            return new RequestStatisticsDto
            {
                TotalRequests = total,
                OpenRequests = open,
                FulfilledRequests = fulfilled,
                CompletedRequests = completed,
            };
        }

        // ── Donation statistics ───────────────────────────────────────────────

        private async Task<DonationStatisticsDto> BuildDonationStatisticsAsync(int? hospitalId)
        {
            ISpecification<Donation> countSpec = hospitalId.HasValue
                ? new HospitalDonationsCountSpecification(hospitalId.Value)
                : new AllDonationsCountSpecification();

            var totalCount = await _uow.Donations.CountAsync(countSpec);

            ISpecification<Donation> statsSpec = hospitalId.HasValue
                ? new HospitalDonationsStatsSpecification(hospitalId.Value)
                : new AllDonationsStatsSpecification();

            var allDonations = await _uow.Donations.GetAllWithSpecAsync(statsSpec);
            var totalQuantity = allDonations.Sum(d =>
                d.BloodRequest is not null ? d.BloodRequest.Quantity : 1);

            return new DonationStatisticsDto
            {
                TotalDonations = totalCount,
                TotalQuantity = totalQuantity,
            };
        }

        // ── User/role resolution ──────────────────────────────────────────────

        /// <summary>
        /// Resolves current user + Role from userId (extracted from JWT).
        /// → KeyNotFoundException   (404) : user not found.
        /// → UnauthorizedAccessException (403) : not AppAdmin or HospitalAdmin.
        /// </summary>
        private async Task<(ApplicationUser user, Role role)> ResolveUserAndRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (user.Role != Role.AppAdmin && user.Role != Role.HospitalAdmin)
                throw new UnauthorizedAccessException(
                    "Access denied. Only AppAdmin and HospitalAdmin can access this endpoint.");

            return (user, user.Role);
        }
    }
}
