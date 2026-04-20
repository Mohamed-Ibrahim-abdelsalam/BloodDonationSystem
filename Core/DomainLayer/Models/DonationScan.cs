using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationSystem.Models
{
    /// <summary>
    /// Confirms hospital received the blood bag. One-to-one with Donation.
    /// </summary>
    public class DonationScan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DonationId { get; set; }

        [ForeignKey(nameof(DonationId))]
        public Donation Donation { get; set; } = null!;

        [Required]
        public int ScannedByHospitalAdminId { get; set; }

        [ForeignKey(nameof(ScannedByHospitalAdminId))]
        public User ScannedByHospitalAdmin { get; set; } = null!;

        public DateTime ScanTime { get; set; } = DateTime.UtcNow;
    }
}
