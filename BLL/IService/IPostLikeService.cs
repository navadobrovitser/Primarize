using System.Collections.Generic;
using DAL.Entities;
using DTO.Likes;

namespace BLL.IServices
{
    public interface IPostLikeService
    {
        IEnumerable<PostLikeDto> GetAllLikes();
        IEnumerable<PostLikeDto> GetLikesByPostId(int postId);
        void AddLike(int userId, int postId, LikeType type);
        void RemoveLike(int userId, int postId);
    }
}