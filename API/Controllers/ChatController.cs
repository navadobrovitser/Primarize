using Azure;
using BLL.IServices;
using DTO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


using Microsoft.AspNetCore.Authorization;


namespace API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public ChatController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        // נקודת קצה רגילה
        [HttpPost]
        public async Task<IActionResult> Ask(ChatRequestDto dto, CancellationToken ct)
        {
            var answer = await _agentService.AskAsync(dto.Messages, ct);
            return Ok(new { text = answer });
        }

        // נקודת קצה בהזרמה (Streaming) באמצעות Server-Sent Events
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
            catch (Exception ex) when (Response.HasStarted && ex is not OperationCanceledException)
            {
                var json = JsonSerializer.Serialize(new { title = "The answer was interrupted. Please try again." });
                await Response.WriteAsync($"event: error\ndata: {json}\n\n", ct);
            }
        }
    }
}