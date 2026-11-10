
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

        // שליפת כל הפוסטים
        // GET: api/Posts
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<PostDto>> GetAllPosts()
        {
            var posts = _postService.GetAllPosts();
            return Ok(posts);
        }

        // שליפת פוסט לפי מזהה
        // GET: api/Posts/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public ActionResult<PostDto> GetPostById(int id)
        {
            var post = _postService.GetPostById(id);
            return Ok(post);
        }

        // יצירת פוסט - רק משתמש מחובר שהוא מועמד
        // POST: api/Posts
        [HttpPost]
        [Authorize]
        public ActionResult CreatePost(
            [FromForm] CreatePostDto model)
        {
            int currentUserId = User.GetUserId();

            var result = _postService.CreatePost(
                model,
                currentUserId
            );

            return Ok(new
            {
                message = "הפוסט נוצר בהצלחה!",
                data = result
            });
        }

        // עדכון פוסט של המועמד המחובר
        // PUT: api/Posts/5
        [HttpPut("{id:int}")]
        [Authorize]
        public IActionResult UpdatePost(
            int id,
            [FromForm] UpdatePostDto model)
        {
            int currentUserId = User.GetUserId();

            _postService.UpdatePost(
                id,
                model,
                currentUserId
            );

            return NoContent();
        }

        // מחיקת פוסט של המועמד המחובר
        // DELETE: api/Posts/5
        [HttpDelete("{id:int}")]
        [Authorize]
        public ActionResult DeletePost(int id)
        {
            int currentUserId = User.GetUserId();

            _postService.DeletePost(
                id,
                currentUserId
            );

            return Ok(new
            {
                message = "הפוסט נמחק בהצלחה!"
            });
        }

        // שליפת הפוסטים של המועמד המחובר בלבד
        // GET: api/Posts/my-posts
        [HttpGet("my-posts")]
        [Authorize]
        public ActionResult<IEnumerable<PostDto>> GetMyPosts()
        {
            int currentUserId = User.GetUserId();

            var posts = _postService.GetPostsByCandidateId(
                currentUserId
            );

            return Ok(posts);
        }
    }
}
