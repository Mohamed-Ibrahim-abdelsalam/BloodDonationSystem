using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Route("api/hospital")]
    [Authorize(Roles = "HospitalAdmin")]
    public class BloodPredictionController : ControllerBase
    {
        private readonly IBloodPredictionService _service;

        public BloodPredictionController(IBloodPredictionService service)
        {
            _service = service;
        }

        // ── GET /api/hospital/predictions?horizonDays=7 ───────────────────────
        /// <summary>
        /// Predict future blood demand for the current hospital.
        /// horizonDays: 7 | 14 | 30 (default: 7)
        /// HospitalId is extracted from JWT — never trusted from client.
        /// </summary>
        [HttpGet("predictions")]
        public async Task<IActionResult> GetPredictions(
            [FromQuery] int horizonDays = 7)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.GetPredictionsAsync(userId, horizonDays);
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
