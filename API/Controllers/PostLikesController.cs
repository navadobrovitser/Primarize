using Microsoft.AspNetCore.Mvc;
using BLL.IServices;
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

        // Get: מחזיר את כל הלייקים במערכת
        [HttpGet]
        public ActionResult<IEnumerable<object>> GetAllLikes()
        {
            var likes = _postLikeService.GetAllLikes();
            return Ok(likes);
        }
    }
}