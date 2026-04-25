using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Authorize]
    public class RewardsController : ControllerBase
    {
        private readonly IRewardService _service;

        public RewardsController(IRewardService service)
        {
            _service = service;
        }

        // ── GET /api/rewards ──────────────────────────────────────────────────
        [HttpGet("api/rewards")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAvailableRewardsAsync();
            return Ok(result);
        }

        // ── GET /api/rewards/{id} ─────────────────────────────────────────────
        [HttpGet("api/rewards/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── POST /api/rewards/redeem ──────────────────────────────────────────
        [HttpPost("api/rewards/redeem")]
        public async Task<IActionResult> Redeem([FromBody] RedeemRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.RedeemAsync(dto.RewardId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── GET /api/users/rewards ────────────────────────────────────────────
        [HttpGet("api/users/rewards")]
        public async Task<IActionResult> GetMyRewards()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.GetMyRewardsAsync(userId);
            return Ok(result);
        }
    }
}
