using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationSystem.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      
        /// Optional: links notification to a specific entity (e.g. RequestId, DonationId).
           
        public int? ReferenceId { get; set; }
    
        /// <summary>
        /// Optional: type of the referenced entity (e.g. "BloodRequest", "Donation", "Reward").
       
        [MaxLength(50)]
        public string? ReferenceType { get; set; }
    }
}
