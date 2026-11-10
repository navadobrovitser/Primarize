using API.Extensions;
using BLL.IServices;
using DTO.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // GET: api/Posts
        [HttpGet]
        [AllowAnonymous]//לבדיקה בריאקט נא למחוק
        public ActionResult<IEnumerable<PostDto>> GetAllPosts()
        {
            var posts = _postService.GetAllPosts();
            return Ok(posts);
        }

        // GET: api/Posts/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]//לבדיקה בריאקט נא למחוק
        public ActionResult<PostDto> GetPostById(int id)
        {
            // ה-Service זורק NotFoundException במידה ואינו מוצא, וה-Filter מחזיר 404
            var post = _postService.GetPostById(id);
            return Ok(post);
        }

        // POST: api/Posts
        [HttpPost]
        [AllowAnonymous]//לבדיקה בריאקט נא למחוק

       // [Authorize(Roles = "Admin,Candidate")]
        public ActionResult CreatePost([FromForm] CreatePostDto model)
        {
            int currentUserId = User.GetUserId();
            var result = _postService.CreatePost(model, currentUserId);
            return Ok(new { message = "הפוסט נוצר בהצלחה!", data = result });
        }

        // PUT: api/Posts/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Candidate")]
        public IActionResult UpdatePost(int id, [FromForm] UpdatePostDto model)
        {
            int currentUserId = User.GetUserId();
            _postService.UpdatePost(id, model, currentUserId);
            return NoContent();
        }

        // DELETE: api/Posts/5
        [HttpDelete("{id:int}")]
        [Authorize]
        public ActionResult DeletePost(int id)
        {
            int currentUserId = User.GetUserId();
            _postService.DeletePost(id, currentUserId);
            return Ok(new { message = "הפוסט נמחק בהצלחה!" });
        }

        // GET: api/Posts/my-posts
        [HttpGet("my-posts")]
        [Authorize]
        public ActionResult<IEnumerable<PostDto>> GetMyPosts()
        {
            int currentUserId = User.GetUserId();
            var posts = _postService.GetPostsByCandidateId(currentUserId);
            return Ok(posts);
        }
    }
}