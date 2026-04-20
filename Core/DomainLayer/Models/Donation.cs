using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BloodDonationSystem.Enums;

namespace BloodDonationSystem.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DonorUserId { get; set; }

        [ForeignKey(nameof(DonorUserId))]
        public User DonorUser { get; set; } = null!;

        [Required]
        public int BloodRequestId { get; set; }

        [ForeignKey(nameof(BloodRequestId))]
        public BloodRequest BloodRequest { get; set; } = null!;

        [Required]
        public int HospitalId { get; set; }

        [ForeignKey(nameof(HospitalId))]
        public Hospital Hospital { get; set; } = null!;

        public BloodType BloodType { get; set; }

         [Required]
        [Range(30, 200, ErrorMessage = "Weight must be between 30 and 200 kg.")]
        public double Weight { get; set; }

        [Required]
        public int age { get; set; }

        [Required]
        public string havetattoos { get; set; }

        public DonationStatus Status { get; set; } = DonationStatus.Pending;

         [MaxLength(200)]
        public string MedicalCondition { get; set; } = string.Empty;

        public DateTime lastDonation { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ─── Navigation properties (one-to-one) ────────────────────────────────
        public DonationScan? DonationScan { get; set; }
        public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
