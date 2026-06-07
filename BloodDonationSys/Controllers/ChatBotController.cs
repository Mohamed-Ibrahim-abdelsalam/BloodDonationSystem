using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System.Security.Claims;

namespace BloodDonationSystem.Controllers
{
    [ApiController]
    [Route("api/chatbot")]
    [Authorize]
    public class ChatBotController : ControllerBase
    {
        private readonly IChatBotService _service;

        public ChatBotController(IChatBotService service)
        {
            _service = service;
        }

        // ── POST /api/chatbot/message ─────────────────────────────────────────
        /// <summary>
        /// Sends a user message to the AI chatbot.
        /// User profile is loaded automatically from JWT — frontend never sends profile data.
        /// </summary>
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _service.SendMessageAsync(userId, dto);
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
                (ex.Message.Contains("invalid response"))
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
