using API.Extensions;
using BLL.IServices;
using DTO.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PostDto>> GetAllPosts()
        {
            return Ok(_postService.GetAllPosts());
        }

        [HttpGet("{id:int}")]
        public ActionResult<PostDto> GetPostById(int id)
        {
            var post = _postService.GetPostById(id);
            if (post == null) return NotFound(new { message = "הפוסט לא נמצא" });
            return Ok(post);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Candidate")]
        public ActionResult CreatePost([FromForm] CreatePostDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // מזהה היוצר נלקח מהטוקן
                int currentUserId = User.GetUserId();
                var result = _postService.CreatePost(model, currentUserId);
                return Ok(new { message = "הפוסט נוצר בהצלחה!", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Candidate")]
        public IActionResult UpdatePost(int id, [FromForm] UpdatePostDto model)
        {
            try
            {
                int currentUserId = User.GetUserId();

                // העדכון מתבצע רק אם הפוסט שייך למשתמש המחובר (נבדק ב-Service באמצעות GetOwned)
                _postService.UpdatePost(id, model, currentUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Candidate")]
        public ActionResult DeletePost(int id)
        {
            try
            {
                int currentUserId = User.GetUserId();

                // המחיקה מתבצעת רק אם הפוסט שייך למשתמש המחובר (נבדק ב-Service באמצעות GetOwned)
                _postService.DeletePost(id, currentUserId);
                return Ok(new { message = "הפוסט נמחק בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // קבלת הפוסטים של המשתמש המחובר
        [HttpGet("my-posts")]
        public ActionResult<IEnumerable<PostDto>> GetMyPosts()
        {
            int currentUserId = User.GetUserId();
            return Ok(_postService.GetPostsByCandidateId(currentUserId));
        }
    }
}