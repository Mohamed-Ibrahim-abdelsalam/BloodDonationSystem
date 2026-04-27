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
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public UserProfileService( UserManager<ApplicationUser> userManager,IMapper mapper, IUnitOfWork uow)
        {
            _userManager = userManager;
            _mapper = mapper;
            _uow = uow;
        }

        // ── GET /api/users/profile ────────────────────────────────────────────
        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            return _mapper.Map<UserProfileDto>(user);
        }


         // ── GET /api/users/dashboard ─────────────────────────────────────────
        public async Task<UserDashboardDto> GetDashboardAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            // Count only Confirmed donations
            var donationsSpec = new DonationsByUserSpecification(userId);
            var donations     = await _uow.Donations.GetAllWithSpecAsync(donationsSpec);
            var totalDonations = donations.Count(d => d.Status == DonationStatus.Confirmed);

            return new UserDashboardDto
            {
                FullName       = user.FullName,
                TotalDonations = totalDonations,
                TotalPoints    = user.Points,
            };
        }
    


        // ── PUT /api/users/profile ────────────────────────────────────────────
        public async Task<UpdateProfileResponseDto> UpdateProfileAsync(
            string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            // Update only allowed fields — Email and NationalId are NOT touched
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Age = dto.Age;
            user.Gender = dto.Gender;
            user.Address = dto.Address;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            var response = _mapper.Map<UpdateProfileResponseDto>(user);
            response.UpdatedAt = DateTime.UtcNow;
            response.Message = "Profile updated successfully";
            return response;
        }
    }
}
