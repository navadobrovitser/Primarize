using DAL.Entities;
using System.Collections.Generic;

namespace DAL.IRepository
{
    public interface IPostLikeRepository
    {
        PostLike? GetById(int id);
        List<PostLike> GetAll();
        void Add(PostLike postLike);
        void Delete(int id);
        void Update(PostLike postLike);
        PostLike? GetByUserAndPost(int userId, int postId);
        List<PostLike> GetByPostId(int postId);
        int CountByType(int postId, LikeType type);
    }
}