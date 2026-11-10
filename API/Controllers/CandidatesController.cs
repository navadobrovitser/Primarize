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
        [Authorize(Roles = "Admin,Candidate")]
        public ActionResult CreateCandidate([FromForm] CreateCandidateDto model)
        {
            // ה-Filter וה-[ApiController] מטפלים בשגיאות ובשדות לא תקינים
            var result = _candidateService.CreateCandidate(model);
            return Ok(new { message = "המועמד נוצר בהצלחה!", data = result });
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
    }
}