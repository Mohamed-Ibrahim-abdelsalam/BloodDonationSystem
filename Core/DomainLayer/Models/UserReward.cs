using DomainLayer.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationSystem.Models
{
    /// <summary>
    /// Join entity for the many-to-many relationship between User and Reward.
    /// </summary>
    public class UserReward
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public int RewardId { get; set; }

        [ForeignKey(nameof(RewardId))]
        public Reward Reward { get; set; } = null!;

        public int PointsUsed { get; set; }

        public UserRewardStatus Status { get; set; } = UserRewardStatus.Unused;

        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

    }
}
