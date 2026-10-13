using BLL.IRepository;
using DAL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace BLL.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
        {
            _context = context;
        }

        public Post? GetById(int id)
        {
            return _context.Posts
                .Include(p => p.Candidate)
                .Include(p => p.PostLikes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefault(p => p.PostId == id);
        }

        public List<Post> GetAll()
        {
            return _context.Posts
                .Include(p => p.Candidate)
                .Include(p => p.PostLikes)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        public void Add(Post post)
        {
            _context.Posts.Add(post);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var post = _context.Posts.Find(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                _context.SaveChanges();
            }
        }

        public void Update(Post post)
        {
            _context.Posts.Update(post);
            _context.SaveChanges();
        }

        public List<Post> GetByCandidateId(int candidateId)
        {
            return _context.Posts
                .Include(p => p.PostLikes)
                .Include(p => p.Comments)
                .Where(p => p.CandidateId == candidateId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        public List<Post> GetByContentType(PostContentType type)
        {
            return _context.Posts
                .Where(p => p.ContentType == type)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }
    }
}
