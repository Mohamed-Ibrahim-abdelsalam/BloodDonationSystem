using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BloodDonationSystem.Enums;

namespace BloodDonationSystem.Models
{
    public class HospitalInventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int HospitalId { get; set; }

        [ForeignKey(nameof(HospitalId))]
        public Hospital Hospital { get; set; } = null!;

        [Required]
        [MaxLength(5)]
        public string BloodType { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        // Status is automatically computed based on Quantity — never set manually.
        public InventoryStatus Status
        {
            get
            {
                if (Quantity == 0) return InventoryStatus.Critical;
                if (Quantity <= 5) return InventoryStatus.Low;
                return InventoryStatus.Available;
            }
            private set { /* EF Core materialisation */ }
        }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Nullable FK: the admin who last updated this record
        public string? UpdatedByAdminId { get; set; }

        [ForeignKey(nameof(UpdatedByAdminId))]
        public ApplicationUser? UpdatedByAdmin { get; set; }

        [Required]
        [MaxLength(20)]
        public string UpdateSource { get; set; } = "Auto";

        // ─── Navigation properties ──────────────────────────────────────────────
        public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
