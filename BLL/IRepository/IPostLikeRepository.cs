using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.IRepository
{
    public interface IPostLikeRepository
    {
        PostLike? GetById(int id);
        List<PostLike> GetAll();
        void Add(PostLike postLike);
        void Delete(int id);
        void Update(PostLike postLike);

        // extra functions
        PostLike? GetByUserAndPost(int userId, int postId);
        List<PostLike> GetByPostId(int postId);
        int CountByType(int postId, LikeType type);
    }
}
