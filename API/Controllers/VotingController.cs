
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

        public VotingController(
            IVotingService votingService,
            ElectionStateService electionState)
        {
            _votingService = votingService;
            _electionState = electionState;
        }

        // שליחת הצבעה - רק כשהבחירות פתוחות
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

        // סטטוס ההצבעה של המשתמש
        [HttpGet("status")]
        [Authorize]
        public ActionResult<VoteStatusDto> GetMyVoteStatus()
        {
            int currentUserId = User.GetUserId();

            return Ok(
                _votingService.GetVoteStatus(currentUserId));
        }

        // האם הבחירות פתוחות או סגורות?
        [HttpGet("election-status")]
        [Authorize]
        public IActionResult GetElectionStatus()
        {
            return Ok(new
            {
                isClosed = _electionState.IsClosed
            });
        }

        // צפייה בתוצאות - רק אחרי הסגירה
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

        // סגירת הבחירות - למנהל בלבד
        [HttpPost("close")]
        [Authorize(Roles = "Admin")]
        public ActionResult<ElectionResultsDto> CloseElection()
        {
            var results = _electionState.CloseElection(
                () => _votingService.GetElectionResults());

            return Ok(results);
        }
    }
}
