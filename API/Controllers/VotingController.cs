using Microsoft.AspNetCore.Mvc;
using BLL.IRepository;
using DTO.Voting;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotingController : ControllerBase
    {
        private readonly IUserVoteRepository _userVoteRepository;
        private readonly ICandidateRepository _candidateRepository;

        public VotingController(IUserVoteRepository userVoteRepository, ICandidateRepository candidateRepository)
        {
            _userVoteRepository = userVoteRepository;
            _candidateRepository = candidateRepository;
        }

        // 1. Post: שליחת פתק הצבעה
        [HttpPost("cast")]
        public ActionResult CastVote([FromBody] CastVoteDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // כאן אפשר להוסיף לוגיקה לבדיקה אם המשתמש כבר הצביע באמצעות _userVoteRepository.HasVoted(...)

            return Ok(new { message = "ההצבעה נקלטה בהצלחה!", data = model });
        }

        // 2. Get לפי ID של משתמש: בדיקת סטטוס הצבעה
        [HttpGet("status/{userId}")]
        public ActionResult<VoteStatusDto> GetVoteStatus(int userId)
        {
            bool hasVoted = _userVoteRepository.HasVoted(userId);

            return Ok(new VoteStatusDto
            {
                HasVoted = hasVoted,
                VoteDate = null // אפשר לעדכן בהתאם לנתוני ה-DB אם צריך
            });
        }

        // 3. Get: הצגת תוצאות הבחירות הכלליות
        [HttpGet("results")]
        public ActionResult<ElectionResultsDto> GetElectionResults()
        {
            int totalVotes = _userVoteRepository.CountVotes();

            var results = new ElectionResultsDto
            {
                TotalVoters = totalVotes,
                Results = new List<CandidateResultDto>()
            };
            return Ok(results);
        }
    }
}