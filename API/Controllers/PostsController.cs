using Microsoft.AspNetCore.Mvc;
using BLL.IServices;
using DTO.Posts;
using System.Collections.Generic;
using System;

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
        public ActionResult DeletePost(int id)
        {
            try
            {
                _postService.DeletePost(id);
                return Ok(new { message = "הפוסט נמחק בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePost(int id, [FromForm] UpdatePostDto model)
        {
            try
            {
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