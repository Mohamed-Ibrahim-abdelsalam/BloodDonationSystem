using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Route("api/hospital")]
    [Authorize(Roles = "HospitalAdmin")]
    public class BloodUsageController : ControllerBase
    {
        private readonly IBloodUsageService _service;

        public BloodUsageController(IBloodUsageService service)
        {
            _service = service;
        }

        // ── GET /api/hospital/blood-usage?period=1month ───────────────────────
        /// <summary>
        /// Blood usage analytics grouped by blood type for the current hospital.
        /// period: 7days | 1month | 3months | 6months (default: 1month)
        /// HospitalId is extracted from JWT — never trusted from client.
        /// </summary>
        [HttpGet("blood-usage")]
        public async Task<IActionResult> GetBloodUsage(
            [FromQuery] string period = "1month")
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.GetBloodUsageAsync(userId, period);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
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
    }
}
