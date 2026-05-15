using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    // ── Input DTOs ────────────────────────────────────────────────────────────

    /// <summary>Body for POST /api/admin/hospitals.</summary>
    public class CreateHospitalDto
    {
        [Required(ErrorMessage = "Hospital name is required.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(150)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }
    }

    /// <summary>Body for PUT /api/admin/hospitals/{id}.</summary>
    public class UpdateHospitalDto
    {
        [Required(ErrorMessage = "Hospital name is required.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(150)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────

    /// <summary>
    /// Full hospital detail — POST and PUT response.
    /// </summary>
    public class HospitalDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Admin dashboard row — GET /api/admin/hospitals list items.
    /// </summary>
    public class HospitalListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Dashboard response wrapper — statistics + list.
    /// </summary>
    public class HospitalsDashboardDto
    {
        public HospitalStatisticsDto Statistics { get; set; } = new();
        public IEnumerable<HospitalListItemDto> Hospitals { get; set; } = new List<HospitalListItemDto>();
    }

    /// <summary>Aggregated stats for the hospitals dashboard.</summary>
    public class HospitalStatisticsDto
    {
        public int TotalHospitals { get; set; }
    }

    /// <summary>
    /// Lightweight item for GET /api/hospitals/dropdown.
    /// Only Id and Name — nothing sensitive exposed to regular users.
    /// </summary>
    public class HospitalDropdownItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
