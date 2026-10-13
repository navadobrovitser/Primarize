using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.IRepository
{
    public interface ICommentRepository
    {
        Comment? GetById(int id);
        List<Comment> GetAll();
        void Add(Comment comment);
        void Delete(int id);
        void Update(Comment comment);

        // extra functions
        List<Comment> GetByPostId(int postId);
        List<Comment> GetByUserId(int userId);
    }
}
