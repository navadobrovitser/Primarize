using Microsoft.AspNetCore.Mvc;
using BLL.IRepository;
using DTO.Candidates;
using System.Collections.Generic;
using AutoMapper;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IMapper _mapper;

        public CandidatesController(ICandidateRepository candidateRepository, IMapper mapper)
        {
            _candidateRepository = candidateRepository;
            _mapper = mapper;
        }

        // 1. Get: מחזיר את כל המועמדים דרך ה-BLL
        [HttpGet]
        public ActionResult<IEnumerable<CandidateListItemDto>> GetAllCandidates()
        {
            var candidates = _candidateRepository.GetAll();
            var dtos = _mapper.Map<IEnumerable<CandidateListItemDto>>(candidates);
            return Ok(dtos);
        }

        // 2. Get לפי ID: מחזיר פרטים מלאים של מועמד בודד
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

        // 3. Post: הוספת מועמד חדש
        [HttpPost]
        public ActionResult CreateCandidate([FromBody] CreateCandidateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var candidate = _mapper.Map<DAL.Entities.Candidate>(model);
            _candidateRepository.Add(candidate);

            return Ok(new { message = "המועמד נוצר בהצלחה!", data = model });
        }
    }
}