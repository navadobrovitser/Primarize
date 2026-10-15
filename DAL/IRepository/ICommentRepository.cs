using DAL.Entities;
using System.Collections.Generic;

namespace DAL.IRepository
{
    public interface ICommentRepository
    {
        Comment? GetById(int id);
        List<Comment> GetAll();
        void Add(Comment comment);
        void Delete(int id);
        void Update(Comment comment);
        List<Comment> GetByPostId(int postId);
        List<Comment> GetByUserId(int userId);
    }
}