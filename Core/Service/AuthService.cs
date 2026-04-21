using BloodDonationSystem.Models;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServiceAbstraction.Dtos.AuthDto;
using ServiceAbstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtService jwtService,
            IConfiguration config)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _config = config;
        }

        // ── Register ─────────────────────────────────────────────────────────
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Check email unique
            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail is not null)
                throw new InvalidOperationException("Email already exists.");

            // Check NationalId unique
            var existingNationalId = await _userManager.Users
                .AnyAsync(u => u.NationalId == dto.NationalId);
            if (existingNationalId)
                throw new InvalidOperationException("NationalId already exists.");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Age = dto.Age,
                Gender = dto.Gender,
                Address = dto.Address,
                NationalId = dto.NationalId,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            await _userManager.AddToRoleAsync(user, "User");

            return await BuildAuthResponseAsync(user, "User", "Registered successfully");
        }

        // ── Login ─────────────────────────────────────────────────────────────
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            return await BuildAuthResponseAsync(user, role);
        }

        // ── Refresh Token ─────────────────────────────────────────────────────
        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidOperationException("Refresh token is required.");

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens
                    .Any(t => t.Token == refreshToken));

            if (user is null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            var token = user.RefreshTokens.First(t => t.Token == refreshToken);

            if (token.IsRevoked)
                throw new UnauthorizedAccessException("Refresh token has been revoked.");

            if (token.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired.");

            // Revoke old token
            token.IsRevoked = true;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user, role);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var expiryDays = int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"]!);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                UserId = user.Id,
            });

            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = _jwtService.GetAccessTokenExpirySeconds(),
            };
        }

        // ── Logout ────────────────────────────────────────────────────────────
        public async Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidOperationException("Refresh token is required.");

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens
                    .Any(t => t.Token == refreshToken));

            if (user is null)
                throw new InvalidOperationException("Invalid refresh token.");

            var token = user.RefreshTokens.First(t => t.Token == refreshToken);
            token.IsRevoked = true;

            await _userManager.UpdateAsync(user);
        }

        // ── Forgot Password ───────────────────────────────────────────────────
        public async Task<string> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new InvalidOperationException("User not found.");

            // Generate reset token — returned directly (no email service)
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            return resetToken;
        }

        // ── Reset Password ────────────────────────────────────────────────────
        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new InvalidOperationException("User not found.");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }
        }

        // ── Change Password ───────────────────────────────────────────────────
        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }
        }

        // ── Private Helpers ───────────────────────────────────────────────────
        private async Task<AuthResponseDto> BuildAuthResponseAsync(
            ApplicationUser user, string role, string? message = null)
        {
            var accessToken = _jwtService.GenerateAccessToken(user, role);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var expiryDays = int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"]!);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                UserId = user.Id,
            });

            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtService.GetAccessTokenExpirySeconds(),
                Message = message,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Role = role,
                },
            };
        }
    }
}
