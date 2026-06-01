using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    // ── Shared nested DTO ─────────────────────────────────────────────────────

    /// <summary>Minimal hospital info embedded inside Hospital Admin responses.</summary>
    public class HospitalAdminHospitalDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
    }

    // ── Input DTOs ────────────────────────────────────────────────────────────
    
    /// <summary>Body for POST /api/admin/hospital-admins.</summary>
    public class CreateHospitalAdminDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number.")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "HospitalId is required.")]
        public int HospitalId { get; set; }
    }

    /// <summary>Body for PUT /api/admin/hospital-admins/{id}.</summary>
    public class UpdateHospitalAdminDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number.")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "HospitalId is required.")]
        public int HospitalId { get; set; }
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────

    /// <summary>POST and PUT response — full detail with message.</summary>
    public class HospitalAdminDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "HospitalAdmin";
        public HospitalAdminHospitalDto? Hospital { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>GET /api/admin/hospital-admins list item.</summary>
    public class HospitalAdminListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public HospitalAdminHospitalDto? Hospital { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>GET /api/admin/hospital-admins/{id} — full detail.</summary>
    public class HospitalAdminDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "HospitalAdmin";
        public HospitalAdminHospitalDto? Hospital { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Dashboard statistics.</summary>
    public class HospitalAdminStatisticsDto
    {
        public int TotalHospitalAdmins { get; set; }
    }

    /// <summary>Full dashboard response — statistics + list.</summary>
    public class HospitalAdminsDashboardDto
    {
        public HospitalAdminStatisticsDto Dashboard { get; set; } = new();
        public IEnumerable<HospitalAdminListItemDto> HospitalAdmins { get; set; }
            = new List<HospitalAdminListItemDto>();
    }
}
