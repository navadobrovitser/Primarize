using API.Extensions;
using AutoMapper;
using DAL.Entities;
using DAL.IRepository;
using DTO.Comment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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

        // הוספת תגובה - ה-UserId נלקח אך ורק מהטוקן
        [HttpPost]
        public IActionResult CreateComment([FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = _mapper.Map<Comment>(dto);

            // מזהה המשתמש מגיע מהטוקן
            comment.UserId = User.GetUserId();
            comment.CreatedAt = DateTime.UtcNow;

            _commentRepository.Add(comment);

            return Ok(new { message = "התגובה נוספה בהצלחה!", data = comment });
        }

        // עדכון תגובה - סינון לפי מזהה התגובה וגם מזהה הבעלים ב-Repository
        [HttpPut("{id:int}")]
        public IActionResult UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            int currentUserId = User.GetUserId();

            // חיפוש תגובה שהיא גם לפי ה-ID וגם שייכת למשתמש המחובר
            var existingComment = _commentRepository.GetOwned(id, currentUserId);
            if (existingComment == null)
            {
                return NotFound(new { message = "התגובה לא נמצאה." });
            }

            existingComment.Content = dto.Content;
            _commentRepository.Update(existingComment);

            return Ok(new { message = "התגובה עודכנה בהצלחה!" });
        }

        // מחיקת תגובה - סינון לפי מזהה התגובה וגם מזהה הבעלים ב-Repository
        [HttpDelete("{id:int}")]
        public IActionResult DeleteComment(int id)
        {
            int currentUserId = User.GetUserId();

            var existingComment = _commentRepository.GetOwned(id, currentUserId);
            if (existingComment == null)
            {
                return NotFound(new { message = "התגובה לא נמצאה." });
            }

            _commentRepository.Delete(id);

            return Ok(new { message = "התגובה נמחקה בהצלחה!" });
        }

        // קבלת כל התגובות של המשתמש המחובר
        [HttpGet("user-comments")]
        public IActionResult GetMyComments()
        {
            int currentUserId = User.GetUserId();
            var comments = _commentRepository.GetByUser(currentUserId);
            var commentDtos = _mapper.Map<List<CommentDto>>(comments);
            return Ok(commentDtos);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetCommentById(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                return NotFound(new { message = "התגובה המבוקשת לא נמצאה." });
            }

            var commentDto = _mapper.Map<CommentDto>(comment);
            return Ok(commentDto);
        }
    }
}