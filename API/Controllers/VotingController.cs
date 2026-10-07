using Microsoft.AspNetCore.Mvc;
using DTO.Voting;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotingController : ControllerBase
    {
        // 1. Post: שליחת פתק הצבעה מלא (דירוג מועמדים בשיטת IRV)
        [HttpPost("cast")]
        public ActionResult CastVote([FromBody] CastVoteDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new { message = "ההצבעה נקלטה בהצלחה!", data = model });
        }

        // 2. Get לפי ID של משתמש: בדיקת סטטוס הצבעה (האם המשתמש כבר הצביע)
        [HttpGet("status/{userId}")]
        public ActionResult<VoteStatusDto> GetVoteStatus(int userId)
        {
            return Ok(new VoteStatusDto
            {
                HasVoted = false,
                VoteDate = null
            });
        }

        // 3. Get: הצגת תוצאות הבחירות הכלליות
        [HttpGet("results")]
        public ActionResult<ElectionResultsDto> GetElectionResults()
        {
            var results = new ElectionResultsDto
            {
                TotalVoters = 0,
                Results = new List<CandidateResultDto>()
            };
            return Ok(results);
        }
    }
}
