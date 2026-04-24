using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BloodDonationSystem.Enums;

namespace BloodDonationSystem.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        // ── Who is donating ───────────────────────────────────────────────────
        [Required]
        public string DonorUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(DonorUserId))]
        public ApplicationUser DonorUser { get; set; } = null!;

        // ── Linked to a request (nullable = general donation) ─────────────────
        public int? BloodRequestId { get; set; }

        [ForeignKey(nameof(BloodRequestId))]
        public BloodRequest? BloodRequest { get; set; }

        // ── Hospital (nullable = no request linked) ───────────────────────────
        public int? HospitalId { get; set; }

        [ForeignKey(nameof(HospitalId))]
        public Hospital? Hospital { get; set; }

        // ── Blood info ────────────────────────────────────────────────────────
        public BloodType BloodType { get; set; }

        [Range(1, 120)]
        public int Age { get; set; }

        [Range(50, 300, ErrorMessage = "Weight must be at least 50 kg.")]
        public double Weight { get; set; }

        public bool HasTattoo { get; set; } = false;

        public DateTime? LastDonationDate { get; set; }

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(500)]
        public string MedicalCondition { get; set; } = string.Empty;

        // ── Status & timestamps ───────────────────────────────────────────────
        public DonationStatus Status { get; set; } = DonationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ConfirmedAt { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────
        public DonationScan? DonationScan { get; set; }
        public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
