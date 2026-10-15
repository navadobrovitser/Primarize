using System.Collections.Generic;

namespace BLL.IServices
{
    public interface IPostLikeService
    {
        IEnumerable<object> GetAllLikes();
        void AddLike(int userId, int postId);
        void RemoveLike(int userId, int postId);
    }
}