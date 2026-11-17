using BLL.IServices;
using DTO.Voting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet]//מחזירה רשימת הצבעות אנונימית
        [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<AnonymousVoteDto>> GetAllAnonymousVotes()
        {
            var votes = _anonymousVoteService.GetAllAnonymousVotes();
            return Ok(votes);
        }

        // GET: api/AnonymousVotes/{id}
        [HttpGet("{id}")]///מחזירה רשימתת הצבעות של ID מסוים
        [Authorize(Roles = "Admin")]
        public ActionResult<AnonymousVoteDto> GetAnonymousVoteById(int id)
        {
            // ה-Service זורק NotFoundException במידה ואינו מוצא, וה-Filter מחזיר 404
            var vote = _anonymousVoteService.GetById(id);
            return Ok(vote);
        }


    }
}