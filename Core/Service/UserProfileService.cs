using AutoMapper;
using BloodDonationSystem.Models;
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

        public UserProfileService(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        // ── GET /api/users/profile ────────────────────────────────────────────
        public async Task<UserProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            return _mapper.Map<UserProfileDto>(user);
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
