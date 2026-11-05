using API.Extensions;
using BLL.IServices;
using DAL.Entities;
using DTO.Likes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostLikesController : ControllerBase
    {
        private readonly IPostLikeService _postLikeService;

        public PostLikesController(IPostLikeService postLikeService)
        {
            _postLikeService = postLikeService;
        }

        // GET: api/PostLikes
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<PostLikeDto>> GetAllLikes()
        {
            var likes = _postLikeService.GetAllLikes();
            return Ok(likes);
        }

        // GET: api/PostLikes/post/5
        [HttpGet("post/{postId}")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<PostLikeDto>> GetLikesByPostId(int postId)
        {
            var likes = _postLikeService.GetLikesByPostId(postId);
            return Ok(likes);
        }

        // POST: api/PostLikes
        [HttpPost]
        public IActionResult AddLike([FromBody] SetPostLikeDto dto)
        {
            if (!Enum.TryParse<LikeType>(dto.Type, true, out var likeType))
            {
                return BadRequest(new { message = "סוג לייק שגוי." });
            }

            int currentUserId = User.GetUserId();
            _postLikeService.AddLike(currentUserId, dto.PostId, likeType);
            return Ok(new { message = "הלייק נוסף בהצלחה" });
        }

        // DELETE: api/PostLikes
        [HttpDelete]
        public IActionResult RemoveLike([FromQuery] int postId)
        {
            int currentUserId = User.GetUserId();
            _postLikeService.RemoveLike(currentUserId, postId);

            return Ok(new { message = "הלייק הוסר בהצלחה" });
        }
    }
}