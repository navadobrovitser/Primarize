using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Comment;
using AutoMapper;
using System.Collections.Generic;
using BLL.Exceptions;

namespace BLL.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository; // חשוב: הגדרת ה-Repository של הפוסטים
        private readonly IMapper _mapper;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository, // חשוב: הזרקה ב-Constructor
            IMapper mapper)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public IEnumerable<CommentDto> GetAllComments()
        {
            var comments = _commentRepository.GetAll();
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public CommentDto GetCommentById(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                throw new NotFoundException($"Comment with id {id} was not found");
            }
            return _mapper.Map<CommentDto>(comment);
        }

        // 3. הוספת תגובה - מתוקן ללא השמה מ-void
        public CommentDto AddComment(CreateCommentDto model, int currentUserId)
        {
            var post = _postRepository.GetById(model.PostId);
            if (post == null)
            {
                throw new NotFoundException($"Post with id {model.PostId} was not found");
            }

            var comment = _mapper.Map<Comment>(model);
            comment.UserId = currentUserId;

            // התיקון כאן: קודם מוסיפים, ואז ממפים את comment הקיים
            _commentRepository.Add(comment);
            return _mapper.Map<CommentDto>(comment);
        }

        public void UpdateComment(int id, UpdateCommentDto model, int currentUserId)
        {
            var comment = _commentRepository.GetOwned(id, currentUserId);
            if (comment == null)
            {
                throw new NotFoundException("התגובה לא נמצאה או שאינה שייכת למשתמש המחובר.");
            }

            _mapper.Map(model, comment);
            _commentRepository.Update(comment);
        }

        public void DeleteComment(int id, int currentUserId, string currentUserRole)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                throw new NotFoundException($"Comment with id {id} was not found");
            }

            bool isOwner = comment.UserId == currentUserId;
            bool isAdmin = currentUserRole == "Admin";

            if (!isOwner && !isAdmin)
            {
                throw new BusinessRuleException("אינך מורשה למחוק תגובה זו.");
            }

            _commentRepository.Delete(id);
        }

        public IEnumerable<CommentDto> GetCommentsByPostId(int postId)
        {
            var comments = _commentRepository.GetByPostId(postId);
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }
    }
}