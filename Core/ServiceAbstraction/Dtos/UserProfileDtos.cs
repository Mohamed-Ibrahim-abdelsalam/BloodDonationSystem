using BloodDonationSystem.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public string Address { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public string Address { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ── Request ───────────────────────────────────────────────────────────────

    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 120)]
        public int Age { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;
    }
}
