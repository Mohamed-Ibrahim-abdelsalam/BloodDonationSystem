using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Interfaces;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Route("api/hospitals")]
    [Authorize]
    public class HospitalsController : ControllerBase
    {
        private readonly IHospitalService _service;

        public HospitalsController(IHospitalService service)
        {
            _service = service;
        }

        // ── GET /api/hospitals/dropdown ───────────────────────────────────────
        /// <summary>
        /// Lightweight id+name list for frontend dropdowns.
        /// Used when creating a blood request or donation.
        /// No sensitive data (email, phone, address) is exposed.
        /// </summary>
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            var result = await _service.GetDropdownAsync();
            return Ok(result);
        }
    }
}
