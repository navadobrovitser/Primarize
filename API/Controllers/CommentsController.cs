using Microsoft.AspNetCore.Mvc;
using BLL.IServices;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // 1. Get: מחזיר את כל התגובות
        [HttpGet]
        public ActionResult<IEnumerable<object>> GetAllComments()
        {
            var comments = _commentService.GetAllComments();
            return Ok(comments);
        }

        // 2. Get לפי ID של תגובה
        [HttpGet("{id}")]
        public ActionResult GetCommentById(int id)
        {
            var comment = _commentService.GetCommentById(id);
            if (comment == null)
            {
                return NotFound(new { message = "התגובה לא נמצאה" });
            }
            return Ok(comment);
        }
    }
}