using System.ComponentModel.DataAnnotations;

namespace BloodDonationSystem.Models
{
    public class Reward
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "RequiredPoints must be a non-negative value.")]
        public int RequiredPoints { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<UserReward> UserRewards { get; set; } = new List<UserReward>();
    }
}
