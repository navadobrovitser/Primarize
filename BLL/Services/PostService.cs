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

        // 1. תיקון יצירת פוסט עם מזהה המועמד מהטוקן
        public PostDto CreatePost(CreatePostDto model, int currentUserId)
        {
            var candidate = _candidateRepository.GetByUserId(currentUserId);
            if (candidate == null)
            {
                throw new KeyNotFoundException("המועמד המבוקש אינו קיים במערכת.");
            }

            var post = _mapper.Map<Post>(model);
            post.CandidateId = candidate.CandidateId;

            if (model.MediaFile != null && model.MediaFile.Length > 0)
            {
                post.MediaUrl = _imageService.Save(model.MediaFile);
            }

            _postRepository.Add(post);

            return _mapper.Map<PostDto>(post);
        }

        // 2. תיקון עדכון פוסט עם בדיקת GetOwned
        public void UpdatePost(int id, UpdatePostDto model, int currentUserId)
        {
            var candidate = _candidateRepository.GetByUserId(currentUserId);
            if (candidate == null)
            {
                throw new KeyNotFoundException("המועמד לא נמצא.");
            }

            var post = _postRepository.GetOwned(id, candidate.CandidateId);
            if (post == null)
            {
                throw new KeyNotFoundException("הפוסט לא נמצא או שאינו שייך למועמד.");
            }

            _mapper.Map(model, post);

            if (model.MediaFile != null && model.MediaFile.Length > 0)
            {
                post.MediaUrl = _imageService.Save(model.MediaFile);
            }

            _postRepository.Update(post);
        }

        // 3. תיקון מחיקת פוסט עם בדיקת GetOwned
        public void DeletePost(int id, int currentUserId)
        {
            var candidate = _candidateRepository.GetByUserId(currentUserId);
            if (candidate == null)
            {
                // משתמש רגיל שאינו מועמד - לא מוצא את הפוסט
                throw new KeyNotFoundException("הפוסט לא נמצא.");
            }

            var post = _postRepository.GetOwned(id, candidate.CandidateId);
            if (post == null)
            {
                // מועמד שמנסה למחוק פוסט שלא שייך לו - לא מוצא את הפוסט
                throw new KeyNotFoundException("הפוסט לא נמצא.");
            }

            _postRepository.Delete(id);
        }
        // 4. הוספת המתודה לקבלת פוסטים לפי מזהה המועמד
        public IEnumerable<PostDto> GetPostsByCandidateId(int currentUserId)
        {
            var candidate = _candidateRepository.GetByUserId(currentUserId);
            if (candidate == null)
            {
                return new List<PostDto>();
            }

            var posts = _postRepository.GetByCandidateId(candidate.CandidateId);
            return _mapper.Map<IEnumerable<PostDto>>(posts);
        }
    }
}