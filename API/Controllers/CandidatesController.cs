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

        // GET: api/Candidates/{id}
        [HttpGet("{id}")]
        public ActionResult<CandidateDto> GetCandidateById(string id)
        {
            // ה-Service זורק NotFoundException במידה ואינו מוצא, וה-Filter מחזיר 404
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

        // PUT: api/Candidates
        [HttpPut]
        [Authorize(Roles = "Admin,Candidate")]
        public ActionResult UpdateCandidate([FromForm] UpdateCandidateDto model)
        {
            int currentUserId = User.GetUserId();
            _candidateService.UpdateCandidate(currentUserId, model);
            return Ok(new { message = "המועמד עודכן בהצלחה!" });
        }

        // DELETE: api/Candidates
        [HttpDelete]
        [Authorize(Roles = "Admin,Candidate")]
        public ActionResult DeleteCandidate()
        {
            int currentUserId = User.GetUserId();
            _candidateService.DeleteCandidate(currentUserId);
            return Ok(new { message = "המועמד נמחק בהצלחה!" });
        }

        [HttpGet("eligibility")]
        [Authorize]
        public IActionResult CheckEligibility()
        {
            int userId = User.GetUserId();

            var result = _candidateService.CheckEligibility(userId);

            return Ok(result);
        }


        [HttpGet("profile/{candidateId:int}")]
        [AllowAnonymous]
        public ActionResult<CandidateDto> GetCandidateProfile(int candidateId)
        {
            var candidate = _candidateService.GetCandidateByCandidateId(candidateId);
            return Ok(candidate);
        }

    }
}