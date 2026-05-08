using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public AdminController(IAdminService service)
        {
            _service = service;
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
    }
}
