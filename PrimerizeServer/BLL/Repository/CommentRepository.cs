using BLL.IRepository;
using DAL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace BLL.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public Comment? GetById(int id)
        {
            return _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .FirstOrDefault(c => c.CommentId == id);
        }

        public List<Comment> GetAll()
        {
            return _context.Comments
                .Include(c => c.User)
                .ToList();
        }

        public void Add(Comment comment)
        {
            _context.Comments.Add(comment);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var comment = _context.Comments.Find(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                _context.SaveChanges();
            }
        }

        public void Update(Comment comment)
        {
            _context.Comments.Update(comment);
            _context.SaveChanges();
        }

        public List<Comment> GetByPostId(int postId)
        {
            return _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToList();
        }

        public List<Comment> GetByUserId(int userId)
        {
            return _context.Comments
                .Where(c => c.UserId == userId)
                .ToList();
        }
    }
}

