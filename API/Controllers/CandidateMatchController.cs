
using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    // =====================================================
    // 1. בקשת התאמת מועמדים
    //
    // הגדרת מבנה הבקשה והגבלת אורך הטקסט
    // שנשלח על ידי המשתמש למנגנון ההתאמה.
    // =====================================================

    public class MatchRequest
    {
        [Required]
        [StringLength(1000, MinimumLength = 3)]
        public string Query { get; set; } = "";
    }

    // =====================================================
    // 2. Candidate Matching API
    //
    // מאפשר למשתמשים מחוברים לקבל התאמת מועמדים
    // בהתאם לדרישות ולהעדפות שהזינו.
    // =====================================================

    [ApiController]
    [Route("api/candidate-match")]
    [Authorize]
    public class CandidateMatchController : ControllerBase
    {
        private readonly ICandidateMatchService _matchService;

        // Dependency Injection
        public CandidateMatchController(
            ICandidateMatchService matchService)
        {
            _matchService = matchService;
        }

        // =====================================================
        // 3. ביצוע התאמת מועמדים
        //
        // העברת בקשת המשתמש לשכבת הלוגיקה העסקית.
        // עיבוד אסינכרוני עם תמיכה בביטול הבקשה.
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Match(
            [FromBody] MatchRequest request,
            CancellationToken ct)
        {
            var result = await _matchService.MatchAsync(
                request.Query,
                ct
            );

            return Ok(result);
        }
    }
}
