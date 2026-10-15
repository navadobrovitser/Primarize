using System.Collections.Generic;

namespace BLL.IServices
{
    public interface ICommentService
    {
        IEnumerable<object> GetAllComments();
        object? GetCommentById(int id);
        void AddComment(object model); // ניתן להתאים לפי DTO מתאים
        void DeleteComment(int id);
    }
}