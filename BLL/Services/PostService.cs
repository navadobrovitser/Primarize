
using AutoMapper;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Posts;
using System;
using System.Collections.Generic;
using System.IO;
using BLL.Exceptions;

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

        public PostDto GetPostById(int id)
        {
            var post = _postRepository.GetById(id);

            if (post == null)
            {
                throw new NotFoundException(
                    $"Post with id {id} was not found"
                );
            }

            return _mapper.Map<PostDto>(post);
        }

        // זיהוי סוג הקובץ לפי הסיומת
        private static PostContentType GetMediaType(string fileName)
        {
            var extension = Path
                .GetExtension(fileName)
                .ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".webp":
                    return PostContentType.Image;

                case ".mp4":
                case ".avi":
                case ".mov":
                    return PostContentType.Video;

                default:
                    throw new ArgumentException(
                        "סוג הקובץ אינו נתמך."
                    );
            }
        }

        // שמירת המדיה בהתאם לסוג הקובץ
        private string SaveMedia(
            Microsoft.AspNetCore.Http.IFormFile file,
            PostContentType type)
        {
            if (type == PostContentType.Image)
            {
                return _imageService.SaveImage(file);
            }

            if (type == PostContentType.Video)
            {
                return _imageService.SaveVideo(file);
            }

            throw new ArgumentException(
                "לא ניתן לשמור מדיה מסוג זה."
            );
        }

        // יצירת פוסט חדש
        public PostDto CreatePost(
            CreatePostDto model,
            int currentUserId)
        {
            var candidate = _candidateRepository
                .GetByUserId(currentUserId);

            if (candidate == null)
            {
                throw new NotFoundException(
                    "המועמד המבוקש אינו קיים במערכת."
                );
            }

            if (string.IsNullOrWhiteSpace(model.Content) ||
                model.Content.Trim().Length > 4000)
            {
                throw new ArgumentException(
                    "תוכן הפוסט אינו תקין."
                );
            }

            if (!Enum.TryParse<PostContentType>(
                    model.ContentType,
                    true,
                    out var contentType) ||
                !Enum.IsDefined(
                    typeof(PostContentType),
                    contentType))
            {
                throw new ArgumentException(
                    "סוג הפוסט אינו תקין."
                );
            }

            var hasMedia =
                model.MediaFile != null &&
                model.MediaFile.Length > 0;

            if (hasMedia)
            {
                var actualType = GetMediaType(
                    model.MediaFile!.FileName
                );

                if (contentType != actualType)
                {
                    throw new ArgumentException(
                        "סוג הפוסט אינו תואם לסוג הקובץ."
                    );
                }
            }
            else if (contentType != PostContentType.Text)
            {
                throw new ArgumentException(
                    "יש לצרף קובץ לפוסט מסוג תמונה או סרטון."
                );
            }

            var post = _mapper.Map<Post>(model);

            post.CandidateId = candidate.CandidateId;
            post.Content = model.Content.Trim();
            post.ContentType = contentType;

            if (hasMedia)
            {
                post.MediaUrl = SaveMedia(
                    model.MediaFile!,
                    contentType
                );
            }
            else
            {
                post.MediaUrl = null;
            }

            _postRepository.Add(post);

            return _mapper.Map<PostDto>(post);
        }

        // עדכון פוסט קיים
        public void UpdatePost(
            int id,
            UpdatePostDto model,
            int currentUserId)
        {
            var candidate = _candidateRepository
                .GetByUserId(currentUserId);

            if (candidate == null)
            {
                throw new NotFoundException(
                    "המועמד לא נמצא."
                );
            }

            var post = _postRepository.GetOwned(
                id,
                candidate.CandidateId
            );

            if (post == null)
            {
                throw new NotFoundException(
                    "הפוסט לא נמצא או שאינו שייך למועמד."
                );
            }

            if (string.IsNullOrWhiteSpace(model.Content) ||
                model.Content.Trim().Length > 4000)
            {
                throw new ArgumentException(
                    "תוכן הפוסט אינו תקין."
                );
            }

            if (!Enum.TryParse<PostContentType>(
                    model.ContentType,
                    true,
                    out var contentType) ||
                !Enum.IsDefined(
                    typeof(PostContentType),
                    contentType))
            {
                throw new ArgumentException(
                    "סוג הפוסט אינו תקין."
                );
            }

            var hasNewMedia =
                model.MediaFile != null &&
                model.MediaFile.Length > 0;

            if (hasNewMedia)
            {
                var actualType = GetMediaType(
                    model.MediaFile!.FileName
                );

                if (contentType != actualType)
                {
                    throw new ArgumentException(
                        "סוג הפוסט אינו תואם לסוג הקובץ."
                    );
                }

                post.MediaUrl = SaveMedia(
                    model.MediaFile!,
                    contentType
                );

                post.ContentType = contentType;
            }
            else
            {
                if (post.MediaUrl != null &&
                    contentType != post.ContentType)
                {
                    throw new ArgumentException(
                        "לא ניתן לשנות את סוג הפוסט בלי להחליף את המדיה."
                    );
                }

                if (post.MediaUrl == null &&
                    contentType != PostContentType.Text)
                {
                    throw new ArgumentException(
                        "יש לצרף מדיה לפוסט מסוג תמונה או סרטון."
                    );
                }

                post.ContentType = contentType;
            }

            post.Content = model.Content.Trim();

            _postRepository.Update(post);
        }

        // מחיקת פוסט
        public void DeletePost(int id, int currentUserId)
        {
            var candidate = _candidateRepository
                .GetByUserId(currentUserId);

            if (candidate == null)
            {
                throw new NotFoundException(
                    "הפוסט לא נמצא."
                );
            }

            var post = _postRepository.GetOwned(
                id,
                candidate.CandidateId
            );

            if (post == null)
            {
                throw new NotFoundException(
                    "הפוסט לא נמצא."
                );
            }

            _postRepository.Delete(id);
        }

        // הפוסטים של המועמד המחובר
        public IEnumerable<PostDto> GetPostsByCandidateId(
            int currentUserId)
        {
            var candidate = _candidateRepository
                .GetByUserId(currentUserId);

            if (candidate == null)
            {
                return new List<PostDto>();
            }

            var posts = _postRepository
                .GetByCandidateId(candidate.CandidateId);

            return _mapper.Map<IEnumerable<PostDto>>(posts);
        }
    }
}
