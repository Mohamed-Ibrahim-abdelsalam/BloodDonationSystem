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
        [Authorize(Roles = "HospitalAdmin")]
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

   
        // ── POST /api/requests/pickup-scan ────────────────────────────────────
        // Target is identified entirely by the QR token — no route id needed
        // Case 1 — BloodRequest pickup   → User (owner) OR HospitalAdmin → Completed
        // Case 2 — General donation withdrawal → HospitalAdmin only → Withdrawn
                [HttpPost("pickup-scan")]
                [Authorize(Roles = "User,HospitalAdmin")]
              public async Task<IActionResult> ScanPickupQr([FromBody] ScanQrDto dto)
               {
                  if (!ModelState.IsValid)
                    return BadRequest(ModelState);
        
                try
                {
                      var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                      var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
                      var result   = await _qrService.ScanPickupQrAsync(dto.QrToken, userId, userRole);
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


         // ── GET /api/hospital/donations/{id}/pickup-qr ─────────────────────────
         // Generate withdrawal QR for a general donation (no BloodRequest)
        [HttpGet("donations/{id:int}/pickup-qr")]
        [Authorize(Roles = "HospitalAdmin")]
        public async Task<IActionResult> GenerateGeneralDonationPickupQr(int id)
        {
           try
              {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _qrService.GenerateGeneralDonationPickupQrAsync(id, userId);
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




       
        // ── POST /api/hospital/donations/scan ─────────────────────────────────
             // Donation is identified by the QR token — no route id needed\n'
                [HttpPost("donations/scan")]
                [Authorize(Roles = "HospitalAdmin")]
                public async Task<IActionResult> ScanDonationQr([FromBody] ScanQrDto dto)
                {
                    if (!ModelState.IsValid)
                        return BadRequest(ModelState);
        
                    try
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                        var result = await _qrService.ScanDonationQrAsync(dto.QrToken, userId);
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
    // Reward QR endpoints
    // ═══════════════════════════════════════════════════════════════════════
    [ApiController]
    [Authorize]
    public class RewardQrController : ControllerBase
    {
        private readonly IQrService _qrService;

        public RewardQrController(IQrService qrService)
        {
            _qrService = qrService;
        }

        // ── GET /api/rewards/redemptions/{id}/qr ─────────────────────────────
        /// <summary>Generate a QR token for a redeemed reward (Unused status only).</summary>
        [HttpGet("api/rewards/redemptions/{id:int}/qr")]
        public async Task<IActionResult> GenerateRewardQr(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _qrService.GenerateRewardQrAsync(id, userId);
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

        // ── POST /api/hospital/rewards/scan ───────────────────────────────────
        /// <summary>Hospital Admin scans reward QR — marks redemption as Used.</summary>
        [HttpPost("api/hospital/rewards/scan")]
        [Authorize(Roles = "HospitalAdmin")]
        public async Task<IActionResult> ScanRewardQr([FromBody] ScanQrDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _qrService.ScanRewardQrAsync(dto.QrToken);
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
