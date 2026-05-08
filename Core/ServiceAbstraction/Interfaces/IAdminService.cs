using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IAdminService
    {
        // ── Requests dashboard ────────────────────────────────────────────────

        /// <summary>
        /// Role-filtered statistics (full set) + paginated request list.
        /// AppAdmin → all requests. HospitalAdmin → their hospital only.
        /// </summary>
        Task<AdminRequestsDashboardDto> GetRequestsDashboardAsync(
            string userId,
            PaginationParams pagination);

        // ── Donations dashboard ───────────────────────────────────────────────

        /// <summary>
        /// Role-filtered statistics (full set) + paginated donation list.
        /// AppAdmin → all donations. HospitalAdmin → their hospital only.
        /// </summary>
        Task<AdminDonationsDashboardDto> GetDonationsDashboardAsync(
            string userId,
            PaginationParams pagination);

        // ── Users management ──────────────────────────────────────────────────

        /// <summary>
        /// GET /api/admin/users — paginated donor list with activity status.
        /// AppAdmin → all users with Role == User.
        /// HospitalAdmin → only donors who have confirmed donations at their hospital.
        /// Ordered by LastDonationDate DESC.
        /// </summary>
        Task<PaginatedResponse<AdminUserListItemDto>> GetUsersAsync(
            string userId,
            PaginationParams pagination);

        /// <summary>
        /// GET /api/admin/users/{id} — full user profile. App Admin only.
        /// Includes TotalDonations count and computed activity status.
        /// </summary>
        Task<AdminUserDetailDto> GetUserByIdAsync(
            string requestingUserId,
            string targetUserId);
    }
}
