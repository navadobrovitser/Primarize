using API.Extensions;
using BLL.IServices;
using DTO.Comment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/Comments
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<CommentDto>> GetAllComments()
        {
            var comments = _commentService.GetAllComments();
            return Ok(comments);
        }

        // GET: api/Comments/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public ActionResult<CommentDto> GetCommentById(int id)
        {
            var comment = _commentService.GetCommentById(id);
            return Ok(comment);
        }

        // GET: api/Comments/post/{postId}
        [HttpGet("post/{postId:int}")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<CommentDto>> GetCommentsByPostId(int postId)
        {
            var comments = _commentService.GetCommentsByPostId(postId);
            return Ok(comments);
        }

        // POST: api/Comments
        [HttpPost]
        public ActionResult<CommentDto> CreateComment([FromBody] CreateCommentDto dto)
        {
            int currentUserId = User.GetUserId();
            var result = _commentService.AddComment(dto, currentUserId);
            return Ok(new { message = "התגובה נוספה בהצלחה!", data = result });
        }

        // PUT: api/Comments/{id}
        [HttpPut("{id:int}")]
        public IActionResult UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            int currentUserId = User.GetUserId();
            _commentService.UpdateComment(id, dto, currentUserId);
            return Ok(new { message = "התגובה עודכנה בהצלחה!" });
        }

        // DELETE: api/Comments/{id}
        [HttpDelete("{id:int}")]
        public IActionResult DeleteComment(int id)
        {
            int currentUserId = User.GetUserId();
            string currentUserRole = User.GetUserRole(); // או העברת Role במידה וקיים Extension method מתאים

            _commentService.DeleteComment(id, currentUserId, currentUserRole);
            return Ok(new { message = "התגובה נמחקה בהצלחה!" });
        }
    }
}