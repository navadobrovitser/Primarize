using Microsoft.AspNetCore.Mvc;
using BLL.IRepository;
using System.Collections.Generic;
using AutoMapper;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public CommentsController(ICommentRepository commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        // 1. Get: מחזיר את כל התגובות
        [HttpGet]
        public ActionResult<IEnumerable<object>> GetAllComments()
        {
            var comments = _commentRepository.GetAll();
            return Ok(comments);
        }

        // 2. Get לפי ID של פוסט או תגובה (תלוי איך הוגדר ב-BLL, נניח לפי ID של תגובה)
        [HttpGet("{id}")]
        public ActionResult GetCommentById(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                return NotFound(new { message = "התגובה לא נמצאה" });
            }
            return Ok(comment);
        }
    }
}