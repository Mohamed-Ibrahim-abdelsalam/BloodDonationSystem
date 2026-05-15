using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Dtos.BloodRequests;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiMatchService _service;

        public AiController(IAiMatchService service)
        {
            _service = service;
        }

        // ── GET /api/ai/match-requests ────────────────────────────────────────
        /// <summary>
        /// Returns AI-ranked blood requests for the authenticated donor.
        /// Uses the donor's blood type and GPS location.
        /// All filters are applied BEFORE sending to the AI service.
        /// </summary>
        [HttpGet("match-requests")]
        public async Task<IActionResult> GetMatchedRequests(
            [FromQuery] BloodRequestQueryParams queryParams)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.GetMatchedRequestsAsync(userId, queryParams);
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
            catch (TimeoutException ex)
            {
                return StatusCode(504, new { message = ex.Message });
            }
            catch (HttpRequestException ex) when
                (ex.Message.Contains("unavailable"))
            {
                return StatusCode(503, new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { message = ex.Message });
            }
            catch (Exception ex) when
                (ex.Message.Contains("invalid response") ||
                 ex.Message.Contains("unreadable"))
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
