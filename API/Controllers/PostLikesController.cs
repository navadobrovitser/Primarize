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
        // הוספת לייק - מזהה המשתמש מגיע אך ורק מהטוקן!
        [HttpPost]
        public IActionResult AddLike([FromBody] SetPostLikeDto dto)
        {
            if (!Enum.TryParse<LikeType>(dto.Type, true, out var likeType))
            {
                return BadRequest("סוג לייק שגוי.");
            }

            // מזהה המשתמש המחובר מהטוקן
            int currentUserId = User.GetUserId();

            _postLikeService.AddLike(currentUserId, dto.PostId, likeType);
            return Ok(new { message = "הלייק נוסף בהצלחה" });
        }

        // DELETE: api/PostLikes
        // הסרת לייק - מזהה המשתמש מגיע אך ורק מהטוקן!
        [HttpDelete]
        public IActionResult RemoveLike([FromQuery] int postId)
        {
            try
            {
                int currentUserId = User.GetUserId();
                _postLikeService.RemoveLike(currentUserId, postId);

                return Ok(new { message = "הלייק הוסר בהצלחה" });
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
    }
}