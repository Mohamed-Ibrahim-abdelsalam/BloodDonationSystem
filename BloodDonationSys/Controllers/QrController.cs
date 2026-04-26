using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    // ═══════════════════════════════════════════════════════════════════════
    // Donation QR endpoints — under /api/donations
    // ═══════════════════════════════════════════════════════════════════════
    [ApiController]
    [Route("api/donations")]
    [Authorize]
    public class DonationsQrController : ControllerBase
    {
        private readonly IQrService _qrService;

        public DonationsQrController(IQrService qrService)
        {
            _qrService = qrService;
        }

        // ── GET /api/donations/{id}/qr ────────────────────────────────────────
        [HttpGet("{id:int}/qr")]
        public async Task<IActionResult> GenerateDonationQr(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _qrService.GenerateDonationQrAsync(id, userId);
                return Ok(result);
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

    // ═══════════════════════════════════════════════════════════════════════
    // Requests QR endpoints — under /api/requests
    // ═══════════════════════════════════════════════════════════════════════
    [ApiController]
    [Route("api/requests")]
    [Authorize]
    public class RequestsQrController : ControllerBase
    {
        private readonly IQrService _qrService;

        public RequestsQrController(IQrService qrService)
        {
            _qrService = qrService;
        }

        // ── GET /api/requests/{id}/pickup-qr ─────────────────────────────────
        [HttpGet("{id:int}/pickup-qr")]
        public async Task<IActionResult> GeneratePickupQr(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _qrService.GeneratePickupQrAsync(id, userId);
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

        // ── POST /api/requests/{id}/pickup-scan ───────────────────────────────
        [HttpPost("{id:int}/pickup-scan")]
        public async Task<IActionResult> ScanPickupQr(int id, [FromBody] ScanQrDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _qrService.ScanPickupQrAsync(id, dto.QrToken, userId);
                return Ok(result);
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

    // ═══════════════════════════════════════════════════════════════════════
    // Hospital scan endpoint — under /api/hospital
    // ═══════════════════════════════════════════════════════════════════════
    [ApiController]
    [Route("api/hospital")]
    [Authorize]
    public class HospitalQrController : ControllerBase
    {
        private readonly IQrService _qrService;

        public HospitalQrController(IQrService qrService)
        {
            _qrService = qrService;
        }

        // ── POST /api/hospital/donations/{id}/scan ────────────────────────────
        [HttpPost("donations/{id:int}/scan")]
        public async Task<IActionResult> ScanDonationQr(int id, [FromBody] ScanQrDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _qrService.ScanDonationQrAsync(id, dto.QrToken, userId);
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
