using BloodDonationSystem.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos.BloodRequests
{
    // ── Request DTOs ──────────────────────────────────────────────────────────

    /// <summary>
    /// Body for POST /api/requests.
    /// hospitalName  is NOT accepted — loaded automatically from DB via hospitalId.
    /// priority      is NOT accepted — calculated automatically from neededBy date.
    /// </summary>
    public class CreateBloodRequestDto
    {
        /// <summary>Selected from a dropdown — name is loaded automatically.</summary>
        [Required(ErrorMessage = "HospitalId is required.")]
        public int HospitalId { get; set; }

        [Required(ErrorMessage = "BloodType is required.")]
        public BloodType BloodType { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }

        /// <summary>Human-readable location string (e.g. "Cairo, Egypt").</summary>
        [Required(ErrorMessage = "HospitalLocation is required.")]
        [MaxLength(300)]
        public string HospitalLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Latitude is required.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        public double Longitude { get; set; }

        /// <summary>Date only — no time component. Must be a future date.</summary>
        [Required(ErrorMessage = "NeededBy date is required.")]
        public DateTime NeededBy { get; set; }
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────

    /// <summary>POST /api/requests response — full detail with message.</summary>
    public class BloodRequestDto
    {
        public int Id { get; set; }
        public int? HospitalId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string HospitalLocation { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? NeededBy { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>GET /api/requests/{id} — full detail including CreatedBy.</summary>
    public class BloodRequestDetailDto
    {
        public int Id { get; set; }
        public int? HospitalId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string HospitalLocation { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? NeededBy { get; set; }
    }

    /// <summary>GET /api/requests/my — current user's requests.</summary>
    public class MyBloodRequestDto
    {
        public int Id { get; set; }
        public int? HospitalId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string HospitalLocation { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? NeededBy { get; set; }
    }

    // ── Query Params ──────────────────────────────────────────────────────────

    public class BloodRequestQueryParams
    {
        public BloodType? BloodType { get; set; }
        public RequestPriority? Priority { get; set; }
        public string? Search { get; set; }
    }
}
