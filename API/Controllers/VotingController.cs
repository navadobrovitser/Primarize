using Microsoft.AspNetCore.Mvc;
using BLL.IServices;
using DTO.Voting;
using System;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotingController : ControllerBase
    {
        private readonly IVotingService _votingService;

        public VotingController(IVotingService votingService)
        {
            _votingService = votingService;
        }

        // 1. Post: שליחת פתק הצבעה
        [HttpPost("cast")]
        //public ActionResult CastVote([FromBody] CastVoteDto model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {
        //        _votingService.CastVote(model);
        //        return Ok(new { message = "ההצבעה נקלטה בהצלחה!", data = model });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }



        //}

        // 2. Get לפי ID של משתמש: בדיקת סטטוס הצבעה
        [HttpGet("status/{userId}")]
        public ActionResult<VoteStatusDto> GetVoteStatus(int userId)
        {
            var status = _votingService.GetVoteStatus(userId);
            return Ok(status);
        }

        // 3. Get: הצגת תוצאות הבחירות הכלליות
        [HttpGet("results")]
        public ActionResult<ElectionResultsDto> GetElectionResults()
        {
            var results = _votingService.GetElectionResults();
            return Ok(results);
        }
    }
}