using AutoMapper;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Posts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public PostService(
            IPostRepository postRepository,
            ICandidateRepository candidateRepository,
            IImageService imageService,
            IMapper mapper)
        {
            _postRepository = postRepository;
            _candidateRepository = candidateRepository;
            _imageService = imageService;
            _mapper = mapper;
        }

        public IEnumerable<PostDto> GetAllPosts()
        {
            var posts = _postRepository.GetAll();
            return _mapper.Map<IEnumerable<PostDto>>(posts);
        }

        public PostDto? GetPostById(int id)
        {
            var post = _postRepository.GetById(id);
            if (post == null) return null;
            return _mapper.Map<PostDto>(post);
        }

        public PostDto CreatePost(CreatePostDto model)
        {
            // בדיקה שהמועמד קיים במסד הנתונים לפי מזהה המועמד
            var candidate = _candidateRepository.GetAll()
                .FirstOrDefault(c => c.CandidateId == model.CandidateId);

            if (candidate == null)
            {
                throw new KeyNotFoundException("המועמד המבוקש אינו קיים במערכת, ולכן לא ניתן ליצור עבורו פוסט.");
            }

            var post = _mapper.Map<Post>(model);

            if (model.MediaFile != null && model.MediaFile.Length > 0)
            {
                post.MediaUrl = _imageService.Save(model.MediaFile);
            }

            // הוספה לבסיס הנתונים (מכיוון ש-Add מחזיר void, נשתמש באובייקט post הישיר)
            _postRepository.Add(post);

            return _mapper.Map<PostDto>(post);
        }

        public void UpdatePost(int id, UpdatePostDto model)
        {
            var post = _postRepository.GetById(id);
            if (post == null)
            {
                throw new KeyNotFoundException("הפוסט לא נמצא.");
            }

            // מיפוי הנתונים החדשים אל הפוסט הקיים (CandidateId נשאר ללא שינוי הודות להגדרת ה-Mapping)
            _mapper.Map(model, post);

            // טיפול בקובץ מדיה חדש אם הועלה
            if (model.MediaFile != null && model.MediaFile.Length > 0)
            {
                post.MediaUrl = _imageService.Save(model.MediaFile);
            }

            _postRepository.Update(post);
        }

        public void DeletePost(int id)
        {
            var post = _postRepository.GetById(id);
            if (post == null)
            {
                throw new KeyNotFoundException("הפוסט לא נמצא.");
            }
            _postRepository.Delete(id);
        }
    }
}