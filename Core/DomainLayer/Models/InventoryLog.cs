using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationSystem.Models
{
    public class InventoryLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int HospitalInventoryId { get; set; }

        [ForeignKey(nameof(HospitalInventoryId))]
        public HospitalInventory HospitalInventory { get; set; } = null!;

        [Required]
        public int HospitalId { get; set; }

        [ForeignKey(nameof(HospitalId))]
        public Hospital Hospital { get; set; } = null!;

        [Required]
        [MaxLength(5)]
        public string BloodType { get; set; } = string.Empty;

        [Required]
        public int ChangeAmount { get; set; }

        [Required]
        public int QuantityAfter { get; set; }

        [Required]
        [MaxLength(30)]
        public string Source { get; set; } = string.Empty;

        // Nullable FK: optional link to a Donation
        public int? DonationId { get; set; }

        [ForeignKey(nameof(DonationId))]
        public Donation? Donation { get; set; }

        // Nullable FK: optional link to a BloodRequest
        public int? BloodRequestId { get; set; }

        [ForeignKey(nameof(BloodRequestId))]
        public BloodRequest? BloodRequest { get; set; }

        // Nullable FK: optional admin who made the change
        public string? ChangedByAdminId { get; set; }

        [ForeignKey(nameof(ChangedByAdminId))]
        public ApplicationUser? ChangedByAdmin { get; set; }

        public string? Notes { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
