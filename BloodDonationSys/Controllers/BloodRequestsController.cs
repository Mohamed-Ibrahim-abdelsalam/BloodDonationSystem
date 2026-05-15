using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Dtos.BloodRequests;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Route("api/requests")]
    [Authorize]
    public class BloodRequestsController : ControllerBase
    {
        private readonly IBloodRequestService _service;

        public BloodRequestsController(IBloodRequestService service)
        {
            _service = service;
        }

        // ── POST /api/requests ────────────────────────────────────────────────
        /// <summary>
        /// Create a new blood request.
        /// Priority is auto-calculated from NeededBy. HospitalName is loaded from DB.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBloodRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        // ── GET /api/requests/my ──────────────────────────────────────────────
        // Declared BEFORE {id} to avoid route conflict with the int-constrained route
        /// <summary>Blood requests created by the currently authenticated user.</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.GetMyRequestsAsync(userId);
            return Ok(result);
        }

        // ── GET /api/requests/{id} ────────────────────────────────────────────
        /// <summary>Single blood request detail. Priority recalculated before returning.</summary>
        [HttpGet("{id:int}")]
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

        // ── DELETE /api/requests/{id} ─────────────────────────────────────────
        /// <summary>Delete an Open blood request. Only the request owner can delete.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _service.DeleteAsync(id, userId);
                return Ok(new { message = "Blood request deleted successfully" });
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
