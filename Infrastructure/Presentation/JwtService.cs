using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ServiceAbstraction.Interfaces;
using BloodDonationSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace Presentation
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateAccessToken(ApplicationUser user, string role)
        {
            var key = new SymmetricSecurityKey(
                             Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email,          user.Email!),
                new Claim(ClaimTypes.Role,           role),
                new Claim("FullName",                user.FullName),
            };

            var expiryMinutes = int.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"]!);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public int GetAccessTokenExpirySeconds()
        {
            var minutes = int.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"]!);
            return minutes * 60;
        }
    }
}
