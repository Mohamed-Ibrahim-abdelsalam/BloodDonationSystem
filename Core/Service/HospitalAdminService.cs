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
    public class HospitalAdminService : IHospitalAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        // Role name must match what's seeded in AuthDbContextSeed
        private const string HospitalAdminRole = "HospitalAdmin";

        public HospitalAdminService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        // ── POST /api/admin/hospital-admins ───────────────────────────────────
        public async Task<HospitalAdminDto> CreateAsync(CreateHospitalAdminDto dto)
        {
            // 1. Validate hospital exists
            var hospitalSpec = new HospitalByIdSpecification(dto.HospitalId);
            var hospital = await _uow.Hospitals.GetEntityWithSpecAsync(hospitalSpec)
                ?? throw new KeyNotFoundException(
                    $"Hospital with id {dto.HospitalId} was not found.");
            
            // 2. Validate hospital not already linked to another admin
               var hospitalAdminSpec = new HospitalAdminByHospitalSpecification(dto.HospitalId);
               var existingAdmin     = await _uow.Users.GetEntityWithSpecAsync(hospitalAdminSpec);
               if (existingAdmin is not null)
                    throw new InvalidOperationException(
                        $"Hospital with id {dto.HospitalId} already has a Hospital Admin assigned. " +
                        "Each hospital can only have one Hospital Admin.");
    
                // 3. Validate unique email\n'
                var existing = await _userManager.FindByEmailAsync(dto.Email);
                if (existing is not null)
                    throw new InvalidOperationException(
                        $"A user with email \'{dto.Email}\' already exists.");

            // 3. Create user via Identity
            var now = DateTime.UtcNow;
            var user = new ApplicationUser
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLower(),
                UserName = dto.Email.Trim().ToLower(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                HospitalId = dto.HospitalId,
                Role = Role.HospitalAdmin,
                // Required fields — sensible defaults for admin-created accounts
                Address = hospital.Address ?? string.Empty,
                NationalId = null,
                EmailConfirmed = true,
                CreatedAt = now,
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException(
                    "Failed to create user: " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));

            // 4. Assign Identity role
            var roleResult = await _userManager.AddToRoleAsync(user, HospitalAdminRole);
            if (!roleResult.Succeeded)
            {
                // Rollback — delete the user to keep state consistent
                await _userManager.DeleteAsync(user);
                throw new InvalidOperationException(
                    "Failed to assign role: " +
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            return new HospitalAdminDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = HospitalAdminRole,
                Hospital = ToHospitalDto(hospital),
                CreatedAt = user.CreatedAt,
                Message = "Hospital Admin created successfully",
            };
        }

        // ── GET /api/admin/hospital-admins ────────────────────────────────────
        public async Task<HospitalAdminsDashboardDto> GetAllAsync()
        {
            var spec = new AllHospitalAdminsSpecification();
            var admins = (await _uow.Users.GetAllWithSpecAsync(spec)).ToList();

            var items = admins.Select(a => new HospitalAdminListItemDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Hospital = a.Hospital is not null ? ToHospitalDto(a.Hospital) : null,
                CreatedAt = a.CreatedAt,
            }).ToList();

            return new HospitalAdminsDashboardDto
            {
                Dashboard = new HospitalAdminStatisticsDto
                {
                    TotalHospitalAdmins = items.Count,
                },
                HospitalAdmins = items,
            };
        }

        // ── GET /api/admin/hospital-admins/{id} ───────────────────────────────
        public async Task<HospitalAdminDetailDto> GetByIdAsync(string id)
        {
            var spec = new HospitalAdminByIdSpecification(id);
            var admin = await _uow.Users.GetEntityWithSpecAsync(spec)
                ?? throw new KeyNotFoundException(
                    $"Hospital Admin with id '{id}' was not found.");

            return new HospitalAdminDetailDto
            {
                Id = admin.Id,
                FullName = admin.FullName,
                Email = admin.Email,
                PhoneNumber = admin.PhoneNumber,
                Role = HospitalAdminRole,
                Hospital = admin.Hospital is not null ? ToHospitalDto(admin.Hospital) : null,
                CreatedAt = admin.CreatedAt,
            };
        }

        

        // ── PUT /api/admin/hospital-admins/{id} ───────────────────────────────
        public async Task<HospitalAdminDto> UpdateAsync(string id, UpdateHospitalAdminDto dto)
        {
            // 1. Fetch admin via UserManager (identity-managed, no EF tracking conflict)
            var admin = await _userManager.FindByIdAsync(id)
                ?? throw new KeyNotFoundException(
                    $"Hospital Admin with id '{id}' was not found.");

            if (admin.Role != Role.HospitalAdmin)
                throw new KeyNotFoundException(
                    $"Hospital Admin with id '{id}' was not found.");

            // 2. Validate hospital exists — use AsNoTracking to avoid tracking conflict
            var hospitalSpec = new HospitalByIdSpecification(dto.HospitalId);
            var hospital = await _uow.Hospitals.GetEntityWithSpecAsNoTrackingAsync(hospitalSpec)
                ?? throw new KeyNotFoundException(
                    $"Hospital with id {dto.HospitalId} was not found.");

            // 3. Validate hospital not already linked to another admin (exclude self)\n'
               if (admin.HospitalId != dto.HospitalId)
               {
                    var hospitalAdminSpec = new HospitalAdminByHospitalSpecification(
                        dto.HospitalId, excludeUserId: id);
                    var existingAdmin = await _uow.Users.GetEntityWithSpecAsync(hospitalAdminSpec);
                    if (existingAdmin is not null)
                        throw new InvalidOperationException(
                            $"Hospital with id {dto.HospitalId} already has a Hospital Admin assigned. " +
                            "Each hospital can only have one Hospital Admin.");
               }
    
                // 4. Validate unique email (exclude self)
                if (!string.Equals(admin.Email, dto.Email.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    var emailUser = await _userManager.FindByEmailAsync(dto.Email);
                    if (emailUser is not null)
                        throw new InvalidOperationException(
                            $"A user with email \'{dto.Email}\' already exists.");
                }

            // 4. Apply changes
            admin.FullName = dto.FullName.Trim();
            admin.UserName = dto.Email.Trim().ToLower();
            admin.Email = dto.Email.Trim().ToLower();
            admin.NormalizedEmail = dto.Email.Trim().ToUpper();
            admin.NormalizedUserName = dto.Email.Trim().ToUpper();
            admin.PhoneNumber = dto.PhoneNumber?.Trim();
            admin.HospitalId = dto.HospitalId;

            var updateResult = await _userManager.UpdateAsync(admin);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException(
                    "Failed to update admin: " +
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));

            return new HospitalAdminDto
            {
                Id = admin.Id,
                FullName = admin.FullName,
                Email = admin.Email,
                PhoneNumber = admin.PhoneNumber,
                Role = HospitalAdminRole,
                Hospital = ToHospitalDto(hospital),
                UpdatedAt = DateTime.UtcNow,
                Message = "Hospital Admin updated successfully",
            };
        }

        
        // ── DELETE /api/admin/hospital-admins/{id} ────────────────────────────
        public async Task DeleteAsync(string id)
        {
            // Use UserManager directly — avoids EF tracking conflicts
            // that occur when fetching via GenericRepository then passing to Identity methods
            var admin = await _userManager.FindByIdAsync(id)
                ?? throw new KeyNotFoundException(
                    $"Hospital Admin with id '{id}' was not found.");

            if (admin.Role != Role.HospitalAdmin)
                throw new KeyNotFoundException(
                    $"Hospital Admin with id '{id}' was not found.");

            // Remove Identity role first
            await _userManager.RemoveFromRoleAsync(admin, HospitalAdminRole);

            // Delete the user
            var deleteResult = await _userManager.DeleteAsync(admin);
            if (!deleteResult.Succeeded)
                throw new InvalidOperationException(
                    "Failed to delete admin: " +
                    string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static HospitalAdminHospitalDto ToHospitalDto(Hospital h) => new()
        {
            Id = h.Id,
            Name = h.Name,
            Address = h.Address,
        };
    }
}
