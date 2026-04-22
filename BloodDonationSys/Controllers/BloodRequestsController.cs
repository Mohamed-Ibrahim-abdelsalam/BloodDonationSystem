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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBloodRequestDto dto)
        {
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
        }

        // ── GET /api/requests ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BloodRequestQueryParams queryParams)
        {
            var result = await _service.GetAllAsync(queryParams);
            return Ok(result);
        }

        // ── GET /api/requests/my ──────────────────────────────────────────────
        // NOTE: must be declared BEFORE {id} to avoid route conflict
        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.GetMyRequestsAsync(userId);
            return Ok(result);
        }

        // ── GET /api/requests/{id} ────────────────────────────────────────────
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
