using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // חובה עבור OpenReadStream ו-IFormFile
using BLL.IRepository;
using BLL.Services;            // חובה עבור FileService
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
        private readonly FileService _fileService; // שדה לשירות הקבצים

        // בנאי המזריק את כל השירותים הנדרשים
        public CandidatesController(
            ICandidateRepository candidateRepository,
            IMapper mapper,
            FileService fileService)
        {
            _candidateRepository = candidateRepository;
            _mapper = mapper;
            _fileService = fileService;
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

        // 3. Post: הוספת מועמד חדש כולל קבצים
        [HttpPost]
        public ActionResult CreateCandidate([FromForm] CreateCandidateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string imageUrl = string.Empty;
            string videoUrl = string.Empty;

            // שירת קובץ התמונה אם קיים בבקשה
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                using (var stream = model.ImageFile.OpenReadStream())
                {
                    imageUrl = _fileService.SaveImage(stream, model.ImageFile.FileName);
                }
            }

            // שירת קובץ הסרטון אם קיים בבקשה
            if (model.ExplanationVideoFile != null && model.ExplanationVideoFile.Length > 0)
            {
                using (var stream = model.ExplanationVideoFile.OpenReadStream())
                {
                    videoUrl = _fileService.SaveImage(stream, model.ExplanationVideoFile.FileName);
                }
            }

            // מיפוי הנתונים ושמירה
            var candidate = _mapper.Map<DAL.Entities.Candidate>(model);

            candidate.ProfileImageUrl = imageUrl;
            candidate.ExplanationVideoUrl = videoUrl;

            _candidateRepository.Add(candidate);

            return Ok(new { message = "המועמד נוצר בהצלחה!", data = candidate });
        }
    }
}