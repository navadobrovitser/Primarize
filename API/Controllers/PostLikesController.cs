using Microsoft.AspNetCore.Mvc;
using BLL.IRepository;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostLikesController : ControllerBase
    {
        private readonly IPostLikeRepository _postLikeRepository;

        public PostLikesController(IPostLikeRepository postLikeRepository)
        {
            _postLikeRepository = postLikeRepository;
        }

        // Get: מחזיר את כל הלייקים במערכת
        [HttpGet]
        public ActionResult<IEnumerable<object>> GetAllLikes()
        {
            var likes = _postLikeRepository.GetAll();
            return Ok(likes);
        }

        // אפשר להוסיף כאן מתודות נוספות במידת הצורך לפי מה שהחברה הגדירה ב-BLL
    }
}