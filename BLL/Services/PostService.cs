using AutoMapper;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using BLL.Exceptions; // הוספת ה-using לחריגות שלנו

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

        // 1. שליפת פוסט לפי מזהה - זריקת NotFoundException במקום null
        public PostDto GetPostById(int id)
        {
            var post = _postRepository.GetById(id);
            if (post == null)
            {
                throw new NotFoundException($"Post with id {id} was not found");
            }
            return _mapper.Map<PostDto>(post);
        }

        // 2. יצירת פוסט - זריקת NotFoundException במידה והמועמד לא קיים במערכת
        public PostDto CreatePost(CreatePostDto model, int currentUserId)
        {
            var candidate = _candidateRepository.GetByUserId(currentUserId);
            if (candidate == null)
            {
                throw new NotFoundException("המועמד המבוקש אינו קיים במערכת.");
            }

            var post = _mapper.Map<Post>(model);
            post.CandidateId = candidate.CandidateId;

            if (model.MediaFile != null && model.MediaFile.Length > 0)
            {
                post.MediaUrl = _imageService.SaveVideo(model.MediaFile);
            }

            _postRepository.Add(post);

            return _mapper.Map<PostDto>(post);
        }

        // 3. עדכון פוסט - זריקת NotFoundException
        public void UpdatePost(int id, UpdatePostDto model, int currentUserId)
        {
            var candidate = _candidateRepository.GetByUserId(currentUserId);
            if (candidate == null)
            {
                throw new NotFoundException("המועמד לא נמצא.");
            }

            var post = _postRepository.GetOwned(id, candidate.CandidateId);
            if (post == null)
            {
                throw new NotFoundException("הפוסט לא נמצא או שאינו שייך למועמד.");
            }

            _mapper.Map(model, post);

            if (model.MediaFile != null && model.MediaFile.Length > 0)
            {
                post.MediaUrl = _imageService.Save(model.MediaFile);
            }

            _postRepository.Update(post);
        }

        // 4. מחיקת פוסט - זריקת NotFoundException
        public void DeletePost(int id, int currentUserId)
        {
            var candidate = _candidateRepository.GetByUserId(currentUserId);
            if (candidate == null)
            {
                throw new NotFoundException("הפוסט לא נמצא.");
            }

            var post = _postRepository.GetOwned(id, candidate.CandidateId);
            if (post == null)
            {
                throw new NotFoundException("הפוסט לא נמצא.");
            }

            _postRepository.Delete(id);
        }

        // 5. קבלת פוסטים לפי מזהה המועמד
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