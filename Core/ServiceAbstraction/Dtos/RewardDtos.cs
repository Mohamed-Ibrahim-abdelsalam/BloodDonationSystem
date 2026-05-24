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
        public int PointsRequired { get; set; }
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




    // ── Admin Reward DTOs ─────────────────────────────────────────────────────

    /// <summary>Body for POST /api/admin/rewards.</summary>
    public class CreateRewardDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PointsRequired must be greater than 0.")]
        public int PointsRequired { get; set; }
    }

    /// <summary>Body for PUT /api/admin/rewards/{id}.</summary>
    public class UpdateRewardDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PointsRequired must be greater than 0.")]
        public int PointsRequired { get; set; }
    }

    /// <summary>POST and PUT admin response.</summary>
    public class AdminRewardDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PointsRequired { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Message { get; set; }
    }
}
