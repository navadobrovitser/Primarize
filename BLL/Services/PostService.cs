using AutoMapper;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Posts;
using System;
using System.Collections.Generic;

namespace BLL.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public PostService(IPostRepository postRepository, IImageService imageService, IMapper mapper)
        {
            _postRepository = postRepository;
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
            var post = _mapper.Map<Post>(model);

            if (model.MediaFile != null && model.MediaFile.Length > 0)
            {
                if (model.ContentType == "Video" || model.ContentType == "3")
                {
                    post.MediaUrl = _imageService.SaveVideo(model.MediaFile);
                }
                else
                {
                    post.MediaUrl = _imageService.Save(model.MediaFile);
                }
            }

            _postRepository.Add(post);
            return _mapper.Map<PostDto>(post);
        }

        public void UpdatePost(int id, CreatePostDto model)
        {
            var post = _postRepository.GetById(id);
            if (post == null)
            {
                throw new KeyNotFoundException("הפוסט לא נמצא.");
            }

            _mapper.Map(model, post);
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