using Microsoft.AspNetCore.Mvc;
using BLL.IRepository;
using BLL.Services;
using DTO.Posts;
using System.Collections.Generic;
using AutoMapper;
using System;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public PostsController(IPostRepository postRepository, IImageService imageService, IMapper mapper)
        {
            _postRepository = postRepository;
            _imageService = imageService;
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

        // 3. Post: יצירת פוסט חדש כולל תמיכה בקובצי מדיה
        [HttpPost]
        public ActionResult CreatePost([FromForm] CreatePostDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = _mapper.Map<DAL.Entities.Post>(model);

            // אם המשתמש צירף קובץ מדיה, נטפל בו לפי סוג התוכן
            if (model.MediaFile != null && model.MediaFile.Length > 0)
            {
                try
                {
                    string savedFileName;

                    // בדיקה האם מדובר בסרטון או בתמונה לפי ה-ContentType
                    if (model.ContentType == "Video" || model.ContentType == "3")
                    {
                        savedFileName = _imageService.SaveVideo(model.MediaFile);
                    }
                    else
                    {
                        savedFileName = _imageService.Save(model.MediaFile);
                    }

                    post.MediaUrl = savedFileName;
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            _postRepository.Add(post);

            return Ok(new { message = "הפוסט נוצר בהצלחה!", data = model });
        }
    }
}