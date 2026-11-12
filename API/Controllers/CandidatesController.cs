using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.IServices;
using DTO.Candidates;
using System.Collections.Generic;
using API.Extensions;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateService _candidateService;

        public CandidatesController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        // GET: api/Candidates
        [HttpGet]
        [AllowAnonymous]//לבדיקה בריאקט נא למחוק
        public ActionResult<IEnumerable<CandidateListItemDto>> GetAllCandidates()
        {
            var candidates = _candidateService.GetAllCandidates();
            return Ok(candidates);
        }


        // GET: api/Candidates/by-id/999999999
        [HttpGet("by-id/{id}")]
        public ActionResult<CandidateDto> GetCandidateById(string id)
        {
            var candidate = _candidateService.GetCandidateById(id);
            return Ok(candidate);
        }

        

        // POST: api/Candidates
        [HttpPost]
        [Authorize]
       
        public ActionResult CreateCandidate([FromForm] CreateCandidateDto model)
        {
            int currentUserId = User.GetUserId();

            var result = _candidateService.CreateCandidate(currentUserId, model);

            return Ok(new
            {
                message = "המועמדות הוגשה בהצלחה!",
                data = result
            });
        }


        [HttpPut]
        [Authorize]
        public ActionResult UpdateCandidate(
            [FromForm] UpdateCandidateDto model)
        {
            int currentUserId = User.GetUserId();

            _candidateService.UpdateCandidate(currentUserId, model);

            return Ok(new
            {
                message = "המועמד עודכן בהצלחה!"
            });
        }


        [HttpGet("me")]
        [Authorize]
        public ActionResult<CandidateDto> GetMyCandidate()
        {
            int currentUserId = User.GetUserId();

            var candidate =
                _candidateService.GetMyCandidate(currentUserId);

            return Ok(candidate);
        }

    }
}