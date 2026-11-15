using BLL.IServices;
using DTO.RegulationChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace API.Controllers
{
    // =====================================================
    // Controller של צ'אט התקנון
    //
    // כל משתמש מחובר ומאושר יכול לשאול שאלות.
    // אין צורך בתפקיד Admin.
    //
    // אימות ה-JWT הקיים ב-Program.cs בודק
    // שהמשתמש קיים, מאושר ואינו מחוק.
    // =====================================================

    [ApiController]
    [Route("api/regulation-chat")]
    [Authorize]
    public class RegulationChatController : ControllerBase
    {
        private readonly IRegulationChatService _chatService;

        public RegulationChatController(
            IRegulationChatService chatService)
        {
            _chatService = chatService;
        }

        // =====================================================
        // שליחת שאלה לצ'אט
        //
        // POST: api/regulation-chat/ask
        //
        // הבקשה מכילה שאלה והיסטוריית שיחה אופציונלית.
        // התשובה מכילה טקסט ומקורות מהתקנון.
        // =====================================================

        [HttpPost("ask")]
        public async Task<IActionResult> Ask(
            [FromBody] RegulationChatRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _chatService.AskAsync(
                    request,
                    cancellationToken);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(503, new
                {
                    message =
                        "מאגר הידע של התקנון אינו זמין כרגע."
                });
            }
        }
    }
}