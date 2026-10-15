using Microsoft.AspNetCore.Mvc;
using BLL.IServices;
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

        // Get: מחזיר את כל ההצבעות האנונימיות
        [HttpGet]
        public ActionResult<IEnumerable<object>> GetAllAnonymousVotes()
        {
            var votes = _anonymousVoteService.GetAllAnonymousVotes();
            return Ok(votes);
        }
    }
}