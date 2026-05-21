using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Route("api/hospital")]
    [Authorize(Roles = "HospitalAdmin")]
    public class HospitalInventoryController : ControllerBase
    {
        private readonly IHospitalInventoryService _service;

        public HospitalInventoryController(IHospitalInventoryService service)
        {
            _service = service;
        }

        // ── GET /api/hospital/inventory ───────────────────────────────────────
        /// <summary>
        /// Returns the current hospital's blood inventory dashboard and per-type breakdown.
        /// Status (High/Low/Critical) is computed dynamically from quantity.
        /// nearestExpiryDate supports FIFO bag usage.
        /// Hospital Admin only — scoped to their linked hospital.
        /// </summary>
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.GetInventoryAsync(userId);
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
    }
}
