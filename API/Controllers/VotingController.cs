using API.Extensions;
using BLL.IServices;
using DTO.Voting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // 1. שליחת פתק הצבעה - מזהה המשתמש נלקח אך ורק מהטוקן
        [HttpPost("cast")]
        public IActionResult CastVote([FromBody] CastVoteDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // חילוץ מזהה המשתמש המחובר מהטוקן בלבד
                int currentUserId = User.GetUserId();

                _votingService.CastVote(currentUserId, model);
                return Ok(new { message = "ההצבעה נקלטה בהצלחה!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. בדיקת סטטוס הצבעה של המשתמש המחובר
        [HttpGet("status")]
        public ActionResult<VoteStatusDto> GetMyVoteStatus()
        {
            int currentUserId = User.GetUserId();
            var status = _votingService.GetVoteStatus(currentUserId);
            return Ok(status);
        }

        // 3. הצגת תוצאות הבחירות הכלליות - פתוח לכולם
        [HttpGet("results")]
        [AllowAnonymous]
        public ActionResult<ElectionResultsDto> GetElectionResults()
        {
            var results = _votingService.GetElectionResults();
            return Ok(results);
        }
    }
}