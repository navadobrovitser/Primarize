
using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    public class MatchRequest
    {
        [Required]
        [StringLength(1000, MinimumLength = 3)]
        public string Query { get; set; } = "";
    }

    [ApiController]
    [Route("api/candidate-match")]
    [Authorize]
    public class CandidateMatchController : ControllerBase
    {
        private readonly ICandidateMatchService _matchService;

        public CandidateMatchController(
            ICandidateMatchService matchService)
        {
            _matchService = matchService;
        }

        [HttpPost]
        public async Task<IActionResult> Match(
            [FromBody] MatchRequest request,
            CancellationToken ct)
        {
            var result = await _matchService.MatchAsync(
                request.Query,
                ct
            );

            return Ok(result);
        }
    }
}
