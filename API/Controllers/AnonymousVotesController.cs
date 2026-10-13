using Microsoft.AspNetCore.Mvc;
using BLL.IRepository;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnonymousVotesController : ControllerBase
    {
        private readonly IAnonymousVoteRepository _anonymousVoteRepository;

        public AnonymousVotesController(IAnonymousVoteRepository anonymousVoteRepository)
        {
            _anonymousVoteRepository = anonymousVoteRepository;
        }

        // Get: מחזיר את כל ההצבעות האנונימיות
        [HttpGet]
        public ActionResult<IEnumerable<object>> GetAllAnonymousVotes()
        {
            var votes = _anonymousVoteRepository.GetAll();
            return Ok(votes);
        }
    }
}