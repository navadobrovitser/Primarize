using DTO.Comment;
using System.Collections.Generic;

namespace BLL.IServices
{
    public interface ICommentService
    {
        IEnumerable<CommentDto> GetAllComments();
        CommentDto GetCommentById(int id);
        CommentDto AddComment(CreateCommentDto model, int currentUserId);
        void UpdateComment(int id, UpdateCommentDto model, int currentUserId);
        void DeleteComment(int id, int currentUserId, string currentUserRole);
        IEnumerable<CommentDto> GetCommentsByPostId(int postId);
    }
}