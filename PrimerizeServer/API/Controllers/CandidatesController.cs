using Microsoft.AspNetCore.Mvc;
using DTO.Candidates;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        // 1. Get: מחזיר את כל המועמדים (למסך הבחירות / רשימה)
        [HttpGet]
        public ActionResult<IEnumerable<CandidateListItemDto>> GetAllCandidates()
        {
            var candidates = new List<CandidateListItemDto>();
            return Ok(candidates);
        }

        // 2. Get לפי ID: מחזיר פרטים מלאים של מועמד בודד
        [HttpGet("{id}")]
        public ActionResult<CandidateDto> GetCandidateById(int id)
        {
            return Ok(new CandidateDto
            {
                CandidateId = id,
                UserId = 1,
                Name = "מועמד לדוגמה",
                Slogan = "השינוי מתחיל כאן"
            });
        }

        // 3. Post: הוספת מועמד חדש
        [HttpPost]
        public ActionResult CreateCandidate([FromBody] CreateCandidateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new { message = "המועמד נוצר בהצלחה!", data = model });
        }
    }
}


