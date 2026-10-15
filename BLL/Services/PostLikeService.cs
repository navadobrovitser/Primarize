using BLL.IServices;
using DAL.IRepository;
using System.Collections.Generic;

namespace BLL.Services
{
    public class PostLikeService : IPostLikeService
    {
        private readonly IPostLikeRepository _postLikeRepository;

        public PostLikeService(IPostLikeRepository postLikeRepository)
        {
            _postLikeRepository = postLikeRepository;
        }

        public IEnumerable<object> GetAllLikes()
        {
            return _postLikeRepository.GetAll();
        }

        public void AddLike(int userId, int postId)
        {
            // לוגיקת הוספת לייק
        }

        public void RemoveLike(int userId, int postId)
        {
            // לוגיקת הסרת לייק
        }
    }
}