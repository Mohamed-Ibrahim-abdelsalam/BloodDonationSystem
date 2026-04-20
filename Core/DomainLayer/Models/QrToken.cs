using System.ComponentModel.DataAnnotations;
using BloodDonationSystem.Enums;

namespace BloodDonationSystem.Models
{
    /// <summary>
    /// Secure token for both Donation and Pickup QR codes.
    /// </summary>
    public class QrToken
    {
        [Key]
        public int Id { get; set; }
        
        public QrTokenType Type { get; set; }

        [Required]
        [MaxLength(100)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // FK
        // One-to-one with either Donation or BloodRequest, depending on Type
        public int? DonationId { get; set; }
        public Donation? Donation { get; set; }

        public int? BloodRequestId { get; set; }
        public BloodRequest? BloodRequest { get; set; }
    }
}
