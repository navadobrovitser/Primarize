using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // חובה להוסיף עבור ההרשאות
using BLL.IServices;
using DTO.Candidates;
using System.Collections.Generic;
using System;
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

        [HttpGet]

        public ActionResult<IEnumerable<CandidateListItemDto>> GetAllCandidates()
        {
            return Ok(_candidateService.GetAllCandidates());
        }

        [HttpGet("{id}")]
        public ActionResult<CandidateDto> GetCandidateById(string id)
        {
            var candidate = _candidateService.GetCandidateById(id);
            if (candidate == null) return NotFound(new { message = "המועמד לא נמצא" });
            return Ok(candidate);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Candidate")]
        public ActionResult CreateCandidate([FromForm] CreateCandidateDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = _candidateService.CreateCandidate(model);
                return Ok(new { message = "המועמד נוצר בהצלחה!", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }






        [HttpPut("{id}")]
        // PUT: api/Candidates
        [HttpPut]
        public ActionResult UpdateCandidate([FromForm] UpdateCandidateDto model)
        {
            try
            {
                int currentUserId = User.GetUserId();
                _candidateService.UpdateCandidate(currentUserId, model);
                return Ok(new { message = "המועמד עודכן בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/Candidates
        [HttpDelete]
        public ActionResult DeleteCandidate()
        {
            try
            {
                int currentUserId = User.GetUserId();
                _candidateService.DeleteCandidate(currentUserId);
                return Ok(new { message = "המועמד נמחק בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}