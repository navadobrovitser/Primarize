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

        [HttpGet("{id}")]
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
                var result = _postService.CreatePost(model);
                return Ok(new { message = "הפוסט נוצר בהצלחה!", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Candidate")]//רק למי ששיך

        [HttpDelete("{id}")]
        public ActionResult DeletePost(int id)
        {
            try
            {
                var post = _postService.GetPostById(id);
                if (post == null)
                {
                    return NotFound(new { message = "הפוסט המבוקש לא נמצא." });
                }

                int currentUserId = User.GetUserId();
                bool isAdmin = User.IsInRole("Admin");

                if (post.CandidateId != currentUserId && !isAdmin)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "אינך מורשה למחוק פוסט זה." });
                }

                _postService.DeletePost(id);
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Candidate")]//רק למי ששיך

        public IActionResult UpdatePost(int id, [FromForm] UpdatePostDto model)
        {
            try
            {
                var existingPost = _postService.GetPostById(id);
                if (existingPost == null)
                {
                    return NotFound(new { message = "הפוסט המבוקש לא נמצא." });
                }
                int currentUserId = User.GetUserId();
                if (existingPost.CandidateId != currentUserId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "אינך מורשה לערוך פוסט זה." });
                }
                _postService.UpdatePost(id, model);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}