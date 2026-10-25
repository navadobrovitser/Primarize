using BLL.IServices;
using DTO.Voting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnonymousVotesController : ControllerBase
    {
        private readonly IAnonymousVoteService _anonymousVoteService;

        public AnonymousVotesController(IAnonymousVoteService anonymousVoteService)
        {
            _anonymousVoteService = anonymousVoteService;
        }

        // GET: api/AnonymousVotes
        [HttpGet]
        [Authorize(Roles = "Admin")]//לוודא שאכן אנו מאשרות אפילו למנהל להציץ בזה

        public ActionResult<IEnumerable<AnonymousVoteDto>> GetAllAnonymousVotes()
        {
            var votes = _anonymousVoteService.GetAllAnonymousVotes();
            return Ok(votes);
        }

        // GET: api/AnonymousVotes/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]

        public ActionResult<AnonymousVoteDto> GetAnonymousVoteById(int id)
        {
            var vote = _anonymousVoteService.GetById(id);
            if (vote == null)
            {
                return NotFound(new { message = "ההצבעה האנונימית המבוקשת לא נמצאה." });
            }
            return Ok(vote);
        }

        // POST: api/AnonymousVotes
        [HttpPost]
        public ActionResult AddAnonymousVote([FromBody] CreateAnonymousVoteDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _anonymousVoteService.AddAnonymousVote(model);
                return Ok(new { message = "ההצבעה האנונימית נקלטה בהצלחה!", data = model });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}