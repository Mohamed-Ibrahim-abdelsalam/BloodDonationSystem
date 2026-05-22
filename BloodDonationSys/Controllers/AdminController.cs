using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "AppAdmin,HospitalAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;
        private readonly IHospitalService _hospitalService;
        private readonly IHospitalAdminService _hospitalAdminService;
        private readonly IRewardAdminService _rewardAdminService;

        public AdminController(IAdminService service, IHospitalService hospitalService, IHospitalAdminService hospitalAdminService, IRewardAdminService rewardAdminService)
        {
            _service = service;
            _hospitalService = hospitalService;
            _hospitalAdminService = hospitalAdminService;
            _rewardAdminService = rewardAdminService;
        }

        // ── GET /api/admin/requests?pageNumber=1&pageSize=5 ───────────────────
        /// <summary>
        /// Dashboard: statistics for the full filtered set + paginated request list.
        /// AppAdmin → all requests. HospitalAdmin → their hospital only.
        /// </summary>
        [HttpGet("requests")]
        public async Task<IActionResult> GetRequestsDashboard(
            [FromQuery] PaginationParams pagination)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.GetRequestsDashboardAsync(userId, pagination);
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

        // ── GET /api/admin/donations?pageNumber=1&pageSize=5 ──────────────────
        /// <summary>
        /// Dashboard: statistics for the full filtered set + paginated donation list.
        /// AppAdmin → all donations. HospitalAdmin → their hospital only.
        /// </summary>
        [HttpGet("donations")]
        public async Task<IActionResult> GetDonationsDashboard(
            [FromQuery] PaginationParams pagination)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.GetDonationsDashboardAsync(userId, pagination);
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

        // ── GET /api/admin/users?pageNumber=1&pageSize=5 ──────────────────────
        /// <summary>
        /// Paginated donor list with activity status.
        /// AppAdmin → all users with Role == User, ordered by LastDonationDate DESC.
        /// HospitalAdmin → only donors who have confirmed donations at their hospital.
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] PaginationParams pagination)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.GetUsersAsync(userId, pagination);
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

        // ── GET /api/admin/users/{id} ─────────────────────────────────────────
        /// <summary>
        /// Full user profile: donations count, activity status, role info.
        /// App Admin only — HospitalAdmin receives 403.
        /// </summary>
        [HttpGet("users/{id}")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.GetUserByIdAsync(userId, id);
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
        }


        // ── POST /api/admin/hospitals ─────────────────────────────────────────
        /// <summary>Create a new hospital. App Admin only.</summary>
        [HttpPost("hospitals")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> CreateHospital([FromBody] CreateHospitalDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _hospitalService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetHospitals), null, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── GET /api/admin/hospitals ──────────────────────────────────────────
        /// <summary>All hospitals with statistics. App Admin only.</summary>
        [HttpGet("hospitals")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> GetHospitals()
        {
            var result = await _hospitalService.GetAllAsync();
            return Ok(result);
        }

        // ── PUT /api/admin/hospitals/{id} ─────────────────────────────────────
        /// <summary>Update a hospital. App Admin only.</summary>
        [HttpPut("hospitals/{id:int}")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> UpdateHospital(int id, [FromBody] UpdateHospitalDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _hospitalService.UpdateAsync(id, dto);
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

        // ── DELETE /api/admin/hospitals/{id} ──────────────────────────────────
        /// <summary>Delete a hospital. App Admin only.</summary>
        [HttpDelete("hospitals/{id:int}")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> DeleteHospital(int id)
        {
            try
            {
                await _hospitalService.DeleteAsync(id);
                return Ok(new { message = "Hospital deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }




        // ══════════════════════════════════════════════════════════════════════
        // Hospital Admin Management — AppAdmin only
        // ══════════════════════════════════════════════════════════════════════

        // ── POST /api/admin/hospital-admins ───────────────────────────────────
        [HttpPost("hospital-admins")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> CreateHospitalAdmin(
            [FromBody] CreateHospitalAdminDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _hospitalAdminService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetHospitalAdminById),
                    new { id = result.Id }, result);
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

        // ── GET /api/admin/hospital-admins ────────────────────────────────────
        [HttpGet("hospital-admins")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> GetHospitalAdmins()
        {
            var result = await _hospitalAdminService.GetAllAsync();
            return Ok(result);
        }

        // ── GET /api/admin/hospital-admins/{id} ───────────────────────────────
        [HttpGet("hospital-admins/{id}")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> GetHospitalAdminById(string id)
        {
            try
            {
                var result = await _hospitalAdminService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── PUT /api/admin/hospital-admins/{id} ───────────────────────────────
        [HttpPut("hospital-admins/{id}")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> UpdateHospitalAdmin(
            string id, [FromBody] UpdateHospitalAdminDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _hospitalAdminService.UpdateAsync(id, dto);
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

        // ── DELETE /api/admin/hospital-admins/{id} ────────────────────────────
        [HttpDelete("hospital-admins/{id}")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> DeleteHospitalAdmin(string id)
        {
            try
            {
                await _hospitalAdminService.DeleteAsync(id);
                return Ok(new { message = "Hospital Admin deleted successfully" });
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




        // ══════════════════════════════════════════════════════════════════════
        // Rewards Management — AppAdmin only
        // ══════════════════════════════════════════════════════════════════════

        // ── POST /api/admin/rewards ───────────────────────────────────────────
        [HttpPost("rewards")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> CreateReward([FromBody] CreateRewardDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _rewardAdminService.CreateAsync(dto);
                return StatusCode(201, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── PUT /api/admin/rewards/{id} ───────────────────────────────────────
        [HttpPut("rewards/{id:int}")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> UpdateReward(int id, [FromBody] UpdateRewardDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _rewardAdminService.UpdateAsync(id, dto);
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

        // ── DELETE /api/admin/rewards/{id} ────────────────────────────────────
        [HttpDelete("rewards/{id:int}")]
        [Authorize(Roles = "AppAdmin")]
        public async Task<IActionResult> DeleteReward(int id)
        {
            try
            {
                await _rewardAdminService.DeleteAsync(id);
                return Ok(new { message = "Reward deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }
}
