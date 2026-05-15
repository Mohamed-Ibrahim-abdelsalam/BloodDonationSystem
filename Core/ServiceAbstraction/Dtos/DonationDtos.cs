using BloodDonationSystem.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    // ── Request ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Body for POST /api/donations.
    /// BloodType is intentionally absent — it is read from the authenticated user's profile.
    /// Address is intentionally absent — it is read from the authenticated user's profile.
    /// </summary>
    public class CreateDonationDto
    {
        /// <summary>
        /// Optional. When provided the donation is linked to that blood request.
        /// When null this is a general donation.
        /// </summary>
        public int? BloodRequestId { get; set; }

        /// <summary>Required. The hospital where the donation will take place.</summary>
        [Required(ErrorMessage = "HospitalId is required.")]
        public int HospitalId { get; set; }

        [Required]
        [Range(18, 60, ErrorMessage = "Age must be between 18 and 60.")]
        public int Age { get; set; }

        [Required]
        [Range(50, 300, ErrorMessage = "Weight must be at least 50 kg.")]
        public double Weight { get; set; }

        public bool HasTattoo { get; set; } = false;

        public DateTime? LastDonationDate { get; set; }

        public bool MedicalCondition { get; set; } = false;
    }

    // ── Response ──────────────────────────────────────────────────────────────

    /// <summary>Donor health data nested inside the POST response.</summary>
    public class DonorDataDto
    {
        public int Age { get; set; }
        public double Weight { get; set; }
        public bool HasTattoo { get; set; }
        public DateTime? LastDonationDate { get; set; }

        /// <summary>Mirrors the boolean sent in the request body.</summary>
        public bool MedicalCondition { get; set; }
    }

    /// <summary>Response for POST /api/donations.</summary>
    public class DonationResponseDto
    {
        public int Id { get; set; }
        public int? BloodRequestId { get; set; }
        public int? HospitalId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;   // formatted: "O+"
        public string Status { get; set; } = string.Empty;
        public DonorDataDto DonorData { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>Single item for GET /api/donations/my.</summary>
    public class MyDonationDto
    {
        public int Id { get; set; }
        public int? BloodRequestId { get; set; }
        public int? HospitalId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;   // formatted: "O+"
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
