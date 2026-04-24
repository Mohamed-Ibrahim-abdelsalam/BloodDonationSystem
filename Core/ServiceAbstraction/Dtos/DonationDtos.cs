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

    public class CreateDonationDto
    {
        public int? BloodRequestId { get; set; }   // null = general donation

        [Required]
        public BloodType BloodType { get; set; }

        [Required]
        [Range(18, 60, ErrorMessage = "Age must be between 18 and 60.")]
        public int Age { get; set; }

        [Required]
        [Range(50, 300, ErrorMessage = "Weight must be at least 50 kg.")]
        public double Weight { get; set; }

        public bool HasTattoo { get; set; } = false;

        public DateTime? LastDonationDate { get; set; }

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(500)]
        public string MedicalCondition { get; set; } = string.Empty;
    }

    // ── Response ──────────────────────────────────────────────────────────────

    public class DonorDataDto
    {
        public int Age { get; set; }
        public double Weight { get; set; }
        public bool HasTattoo { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public string MedicalCondition { get; set; } = string.Empty;
    }

    public class DonationResponseDto
    {
        public int Id { get; set; }
        public int? BloodRequestId { get; set; }
        public BloodType BloodType { get; set; }
        public DonationStatus Status { get; set; }
        public DonorDataDto DonorData { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? Message { get; set; }
    }

    public class MyDonationDto
    {
        public int Id { get; set; }
        public int? BloodRequestId { get; set; }
        public BloodType BloodType { get; set; }
        public string? HospitalName { get; set; }    // null for general donations
        public DonationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
