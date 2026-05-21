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

        /// <summary>
        /// Nearest expiry date among available blood bags of this type.
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Computed dynamically from Quantity — never stored.
        /// High: >= 10 | Low: 3–9 | Critical: < 3
        /// </summary>
        [NotMapped]
        public InventoryStatus Status => Quantity switch
        {
            >= 10 => InventoryStatus.High,
            >= 3 => InventoryStatus.Low,
            _ => InventoryStatus.Critical,
        };

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? UpdatedByAdminId { get; set; }

        [ForeignKey(nameof(UpdatedByAdminId))]
        public ApplicationUser? UpdatedByAdmin { get; set; }

        [Required]
        [MaxLength(20)]
        public string UpdateSource { get; set; } = "Auto";

        public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
}
