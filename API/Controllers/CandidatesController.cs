using Microsoft.AspNetCore.Mvc;
using BLL.IServices;
using DTO.Candidates;
using System.Collections.Generic;
using System;

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
        public ActionResult UpdateCandidate(string id, [FromForm] UpdateCandidateDto model)
        {
            try
            {
                _candidateService.UpdateCandidate(id, model);
                return Ok(new { message = "המועמד עודכן בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteCandidate(string id) // שינוי מ-int ל-string
        {
            try
            {
                _candidateService.DeleteCandidate(id);
                return Ok(new { message = "המועמד נמחק בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}