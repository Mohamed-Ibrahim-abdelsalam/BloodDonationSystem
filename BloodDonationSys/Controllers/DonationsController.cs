using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Route("api/donations")]
    [Authorize]
    public class DonationsController : ControllerBase
    {
        private readonly IDonationService _service;

        public DonationsController(IDonationService service)
        {
            _service = service;
        }

        // ── POST /api/donations ───────────────────────────────────────────────
        /// <summary>
        /// Create a donation (request-based or general).
        /// BloodType is automatically taken from the authenticated user's profile.
        /// bloodRequestId is optional — omit for general donations.
        /// hospitalId is required.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDonationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetMy), null, result);
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

        // ── GET /api/donations/my ─────────────────────────────────────────────
        /// <summary>
        /// Get all donations for the currently authenticated user, newest first.
        /// Covers both request-based and general donations.
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.GetMyDonationsAsync(userId);
            return Ok(result);
        }

        // ── POST /api/donations/{id}/cancel ───────────────────────────────────
        /// <summary>
        /// Cancel a pending donation. Only the donor can cancel their own donation.
        /// </summary>
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _service.CancelAsync(id, userId);
                return Ok(new { message = "Donation cancelled successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
