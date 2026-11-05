using BLL.IServices;
using DTO;
using GenerativeAI.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers
{
    [AllowAnonymous]//צריך להוריד את זה זה בשביל הבדיקות
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public ChatController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        private static bool IsOverloaded(ApiException ex) =>
            ex.Message.Contains("503") || ex.Message.Contains("UNAVAILABLE");

        // נקודת קצה רגילה
        [HttpPost]
        public async Task<IActionResult> Ask(ChatRequestDto dto, CancellationToken ct)
        {
            try
            {
                var answer = await _agentService.AskAsync(dto.Messages, ct);
                return Ok(new { text = answer });
            }
            catch (ApiException ex) when (IsOverloaded(ex))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    title = "השירות עמוס כרגע. נסה שוב בעוד רגע."
                });
            }
        }

        // נקודת קצה בהזרמה (Streaming)
        [HttpPost("stream")]
        public async Task Stream(ChatRequestDto dto, CancellationToken ct)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";

            try
            {
                await foreach (var chunk in _agentService.AskStreamingAsync(dto.Messages, ct))
                {
                    var json = JsonSerializer.Serialize(new { text = chunk });
                    await Response.WriteAsync($"data: {json}\n\n", ct);
                    await Response.Body.FlushAsync(ct);
                }

                await Response.WriteAsync("data: [DONE]\n\n", ct);
            }
            catch (ApiException ex) when (IsOverloaded(ex))
            {
                await WriteSseError("השירות עמוס כרגע. נסה שוב בעוד רגע.", ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await WriteSseError("התשובה נקטעה. נסה שוב.", ct);
            }
        }

        private async Task WriteSseError(string message, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(new { title = message });
            await Response.WriteAsync($"event: error\ndata: {json}\n\n", ct);
        }
    }
}