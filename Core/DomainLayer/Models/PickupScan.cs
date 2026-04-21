using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationSystem.Models
{
    /// <summary>
    /// Confirms request owner received blood bag. One-to-one with BloodRequest.
    /// </summary>
    public class PickupScan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BloodRequestId { get; set; }

        [ForeignKey(nameof(BloodRequestId))]
        public BloodRequest BloodRequest { get; set; } = null!;

        [Required]
        public string ScannedByUserId { get; set; }

        [ForeignKey(nameof(ScannedByUserId))]
        public ApplicationUser ScannedByUser { get; set; } = null!;

        public DateTime ScanTime { get; set; } = DateTime.UtcNow;
    }
}
