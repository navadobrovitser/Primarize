using Microsoft.AspNetCore.Mvc;
using BLL.IRepository;
using DTO.Posts;
using System.Collections.Generic;
using AutoMapper;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public PostsController(IPostRepository postRepository, IMapper mapper)
        {
            _postRepository = postRepository;
            _mapper = mapper;
        }

        // 1. Get: מחזיר את כל הפוסטים
        [HttpGet]
        public ActionResult<IEnumerable<PostDto>> GetAllPosts()
        {
            var posts = _postRepository.GetAll();
            var dtos = _mapper.Map<IEnumerable<PostDto>>(posts);
            return Ok(dtos);
        }

        // 2. Get לפי ID: מחזיר פוסט בודד לפי מזהה
        [HttpGet("{id}")]
        public ActionResult<PostDto> GetPostById(int id)
        {
            var post = _postRepository.GetById(id);
            if (post == null)
            {
                return NotFound(new { message = "הפוסט לא נמצא" });
            }
            var dto = _mapper.Map<PostDto>(post);
            return Ok(dto);
        }

        // 3. Post: יצירת פוסט חדש
        [HttpPost]
        public ActionResult CreatePost([FromBody] CreatePostDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = _mapper.Map<DAL.Entities.Post>(model);
            _postRepository.Add(post);

            return Ok(new { message = "הפוסט נוצר בהצלחה!", data = model });
        }
    }
}