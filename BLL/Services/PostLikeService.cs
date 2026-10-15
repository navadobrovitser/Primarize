using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using AutoMapper;
using DTO.Likes;
using System.Collections.Generic;

namespace BLL.Services
{
    public class PostLikeService : IPostLikeService
    {
        private readonly IPostLikeRepository _postLikeRepository;
        private readonly IMapper _mapper;

        public PostLikeService(IPostLikeRepository postLikeRepository, IMapper mapper)
        {
            _postLikeRepository = postLikeRepository;
            _mapper = mapper;
        }

        public IEnumerable<PostLikeDto> GetAllLikes()
        {
            var likes = _postLikeRepository.GetAll();
            return _mapper.Map<IEnumerable<PostLikeDto>>(likes);
        }

        public IEnumerable<PostLikeDto> GetLikesByPostId(int postId)
        {
            var likes = _postLikeRepository.GetByPostId(postId);
            return _mapper.Map<IEnumerable<PostLikeDto>>(likes);
        }

        public void AddLike(int userId, int postId, LikeType type)
        {
            var existingLike = _postLikeRepository.GetByUserAndPost(userId, postId);
            if (existingLike != null)
            {
                existingLike.Type = type;
                _postLikeRepository.Update(existingLike);
            }
            else
            {
                var newLike = new PostLike
                {
                    UserId = userId,
                    PostId = postId,
                    Type = type
                };
                _postLikeRepository.Add(newLike);
            }
        }

        public void RemoveLike(int userId, int postId)
        {
            var existingLike = _postLikeRepository.GetByUserAndPost(userId, postId);
            if (existingLike != null)
            {
                _postLikeRepository.Delete(existingLike.PostLikeId);
            }
        }
    }
}