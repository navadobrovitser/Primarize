using Microsoft.AspNetCore.Mvc;
using BLL.IServices;
using System.Collections.Generic;
using DTO.Likes;
using DAL.Entities;
using System;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostLikesController : ControllerBase
    {
        private readonly IPostLikeService _postLikeService;

        public PostLikesController(IPostLikeService postLikeService)
        {
            _postLikeService = postLikeService;
        }

        // GET: api/PostLikes
        [HttpGet]
        public ActionResult<IEnumerable<PostLikeDto>> GetAllLikes()
        {
            var likes = _postLikeService.GetAllLikes();
            return Ok(likes);
        }

        // GET: api/PostLikes/post/5
        [HttpGet("post/{postId}")]
        public ActionResult<IEnumerable<PostLikeDto>> GetLikesByPostId(int postId)
        {
            var likes = _postLikeService.GetLikesByPostId(postId);
            return Ok(likes);
        }

        // POST: api/PostLikes
        [HttpPost]
        public IActionResult AddLike([FromQuery] int userId, [FromBody] SetPostLikeDto dto)
        {
            if (!Enum.TryParse<LikeType>(dto.Type, true, out var likeType))
            {
                return BadRequest("סוג לייק שגוי.");
            }

            _postLikeService.AddLike(userId, dto.PostId, likeType);
            return Ok(new { message = "הלייק נוסף בהצלחה" });
        }

        // DELETE: api/PostLikes
        [HttpDelete]
        public IActionResult RemoveLike([FromQuery] int userId, [FromQuery] int postId)
        {
            _postLikeService.RemoveLike(userId, postId);
            return Ok(new { message = "הלייק הוסר בהצלחה" });
        }
    }
}