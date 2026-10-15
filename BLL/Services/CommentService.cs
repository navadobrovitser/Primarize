using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using System.Collections.Generic;

namespace BLL.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public IEnumerable<object> GetAllComments()
        {
            return _commentRepository.GetAll();
        }

        public object? GetCommentById(int id)
        {
            return _commentRepository.GetById(id);
        }

        public void AddComment(object model)
        {
            // לוגיקה להוספת תגובה
        }

        public void DeleteComment(int id)
        {
            _commentRepository.Delete(id);
        }
    }
}