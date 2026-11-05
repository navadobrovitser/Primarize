using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using AutoMapper;
using DTO.Likes;
using System.Collections.Generic;
using BLL.Exceptions; // הוספת ה-using לחריגות שלנו

namespace BLL.Services
{
    public class PostLikeService : IPostLikeService
    {
        private readonly IPostLikeRepository _postLikeRepository;
        private readonly IPostRepository _postRepository; // הוספנו בדיקה מול הפוסטים
        private readonly IMapper _mapper;

        public PostLikeService(
            IPostLikeRepository postLikeRepository,
            IPostRepository postRepository,
            IMapper mapper)
        {
            _postLikeRepository = postLikeRepository;
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public IEnumerable<PostLikeDto> GetAllLikes()
        {
            var likes = _postLikeRepository.GetAll();
            return _mapper.Map<IEnumerable<PostLikeDto>>(likes);
        }

        // שליפת לייקים לפי פוסט - עם בדיקת קיום הפוסט
        public IEnumerable<PostLikeDto> GetLikesByPostId(int postId)
        {
            var post = _postRepository.GetById(postId);
            if (post == null)
            {
                throw new NotFoundException($"Post with id {postId} was not found");
            }

            var likes = _postLikeRepository.GetByPostId(postId);
            return _mapper.Map<IEnumerable<PostLikeDto>>(likes);
        }

        // הוספת/עדכון לייק - עם בדיקת קיום הפוסט
        public void AddLike(int userId, int postId, LikeType type)
        {
            var post = _postRepository.GetById(postId);
            if (post == null)
            {
                throw new NotFoundException($"Post with id {postId} was not found");
            }

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

        // הסרת לייק - עם בדיקה שהלייק והפוסט קיימים
        public void RemoveLike(int userId, int postId)
        {
            var existingLike = _postLikeRepository.GetByUserAndPost(userId, postId);
            if (existingLike == null)
            {
                throw new NotFoundException("הלייק המבוקש לא נמצא במערכת.");
            }

            _postLikeRepository.Delete(existingLike.PostLikeId);
        }
    }
}