using Microsoft.AspNetCore.Mvc;
using BLL.IRepository;
using DTO.Candidates;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IMapper _mapper;

        // בנאי המזריק את השירותים הנדרשים
        public CandidatesController(
            ICandidateRepository candidateRepository,
            IMapper mapper)
        {
            _candidateRepository = candidateRepository;
            _mapper = mapper;
        }

        // 1. Get: מחזיר את כל המועמדים
        [HttpGet]
        public ActionResult<IEnumerable<CandidateListItemDto>> GetAllCandidates()
        {
            var candidates = _candidateRepository.GetAll();
            var dtos = _mapper.Map<IEnumerable<CandidateListItemDto>>(candidates);
            return Ok(dtos);
        }

        // 2. Get לפי ID: מחזיר פרטי מועמד בודד
        [HttpGet("{id}")]
        public ActionResult<CandidateDto> GetCandidateById(int id)
        {
            var candidate = _candidateRepository.GetById(id);
            if (candidate == null)
            {
                return NotFound(new { message = "המועמד לא נמצא" });
            }
            var dto = _mapper.Map<CandidateDto>(candidate);
            return Ok(dto);
        }

        // 3. Post: הוספת מועמד חדש כולל קבצים (תמונה וסרטון)
        [HttpPost]
     
        public ActionResult CreateCandidate([FromForm] CreateCandidateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // מיפוי הנתונים הבסיסיים לישות ה-Candidate
                var candidate = _mapper.Map<DAL.Entities.Candidate>(model);

                // כאן התיקון הנדרש: שיוך מזהה המשתמש באופן ידני 
                // (לצורכי בדיקה כרגע, יש להציב את ה-UserId של המשתמש שיצרת קודם במסד הנתונים, למשל 1)
                candidate.UserId = 1;

                // שליחה לריפוזיטורי יחד עם קובץ התמונה וקובץ הסרטון מתוך ה-DTO
                _candidateRepository.Add(candidate, model.ImageFile, model.VideoFile);

                return Ok(new { message = "המועמד נוצר בהצלחה!", data = candidate });
            }
            catch (ArgumentException ex)
            {
                // טיפול בשגיאות הקשורות לסוג קובץ לא חוקי או גודל חריג
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}