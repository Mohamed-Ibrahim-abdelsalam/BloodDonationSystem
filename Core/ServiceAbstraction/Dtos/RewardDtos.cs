using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    // ── Response DTOs ─────────────────────────────────────────────────────────

    /// <summary>GET /api/rewards — catalog list</summary>
    public class RewardDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }

    /// <summary>GET /api/rewards/{id} — detail</summary>
    public class RewardDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PointsRequired { get; set; }
        public bool IsAvailable { get; set; }
    }

    /// <summary>POST /api/rewards/redeem — response</summary>
    public class RedeemResponseDto
    {
        public int RewardId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int PointsUsed { get; set; }
        public int RemainingPoints { get; set; }
        public DateTime RedeemedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>GET /api/users/rewards — user history item</summary>
    public class UserRewardDto
    {
        public int Id { get; set; }
        public int RewardId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int PointsUsed { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RedeemedAt { get; set; }
    }

    // ── Request DTOs ──────────────────────────────────────────────────────────

    public class RedeemRequestDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int RewardId { get; set; }
    }
}
