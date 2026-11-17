
using API.Extensions;
using API.Services;
using BLL.IServices;
using DTO.Voting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotingController : ControllerBase
    {
        private readonly IVotingService _votingService;
        private readonly ElectionStateService _electionState;

        // Dependency Injection
        public VotingController(
            IVotingService votingService,
            ElectionStateService electionState)
        {
            _votingService = votingService;
            _electionState = electionState;
        }

        // =====================================================
        // 1. ביצוע הצבעה
        //
        // מאפשר למשתמש מחובר להצביע כל עוד הבחירות פתוחות.
        // ביצוע ההצבעה מתואם עם מנגנון סגירת הבחירות.
        // =====================================================

        [HttpPost("cast")]
        [Authorize]
        public IActionResult CastVote([FromBody] CastVoteDto model)
        {
            int currentUserId = User.GetUserId();

            if (_electionState.IsClosed)
            {
                return Conflict(new
                {
                    message = "ההצבעה הסתיימה."
                });
            }

            try
            {
                _electionState.ExecuteVote(() =>
                    _votingService.CastVote(currentUserId, model));
            }
            catch (InvalidOperationException)
                when (_electionState.IsClosed)
            {
                return Conflict(new
                {
                    message = "ההצבעה הסתיימה."
                });
            }

            return Ok(new
            {
                message = "ההצבעה נקלטה בהצלחה!"
            });
        }

        // =====================================================
        // 2. סטטוס הצבעה אישי
        //
        // בודק האם המשתמש המחובר כבר הצביע.
        // הזיהוי מתבצע לפי ה-JWT ולא לפי מזהה שנשלח מהלקוח.
        // =====================================================

        [HttpGet("status")]
        [Authorize]
        public ActionResult<VoteStatusDto> GetMyVoteStatus()
        {
            int currentUserId = User.GetUserId();

            return Ok(
                _votingService.GetVoteStatus(currentUserId));
        }

        // =====================================================
        // 3. מצב הבחירות
        //
        // מחזיר האם הבחירות פתוחות או סגורות.
        // משמש את צד הלקוח להתאמת הממשק למצב הבחירות.
        // =====================================================

        [HttpGet("election-status")]
        [Authorize]
        public IActionResult GetElectionStatus()
        {
            return Ok(new
            {
                isClosed = _electionState.IsClosed
            });
        }

        // =====================================================
        // 4. הצגת תוצאות הבחירות
        //
        // התוצאות נחשפות למשתמשים רק לאחר סגירת הבחירות.
        // =====================================================

        [HttpGet("results")]
        [Authorize]
        public ActionResult<ElectionResultsDto> GetElectionResults()
        {
            if (!_electionState.IsClosed)
            {
                return StatusCode(403, new
                {
                    message = "התוצאות יתפרסמו לאחר סגירת ההצבעה."
                });
            }

            return Ok(_electionState.GetResults());
        }

        // =====================================================
        // 5. סגירת הבחירות - מנהל בלבד
        //
        // סגירת ההצבעה וחישוב התוצאות מתבצעים
        // באמצעות ElectionStateService ו-VotingService.
        // =====================================================

        [HttpPost("close")]
        [Authorize(Roles = "Admin")]
        public ActionResult<ElectionResultsDto> CloseElection()
        {
            var results = _electionState.CloseElection(
                () => _votingService.GetElectionResults());

            return Ok(results);
        }

        // =====================================================
        // 6. פתיחת הבחירות מחדש - מנהל בלבד
        //
        // מאפשר למנהל לשנות את מצב הבחירות לפתוחות.
        // =====================================================

        [HttpPost("open")]
        [Authorize(Roles = "Admin")]
        public IActionResult OpenElection()
        {
            _electionState.OpenElection();

            return Ok(new
            {
                message = "הבחירות נפתחו מחדש בהצלחה.",
                isClosed = false
            });
        }
    }
}
