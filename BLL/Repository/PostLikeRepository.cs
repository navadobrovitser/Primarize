using BLL.IRepository;
using DAL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace BLL.Repository
{
    public class PostLikeRepository : IPostLikeRepository
    {
        private readonly AppDbContext _context;

        public PostLikeRepository(AppDbContext context)
        {
            _context = context;
        }

        public PostLike? GetById(int id)
        {
            return _context.PostLikes
                .Include(l => l.User)
                .Include(l => l.Post)
                .FirstOrDefault(l => l.PostLikeId == id);
        }

        public List<PostLike> GetAll()
        {
            return _context.PostLikes.ToList();
        }

        public void Add(PostLike postLike)
        {
            _context.PostLikes.Add(postLike);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var like = _context.PostLikes.Find(id);
            if (like != null)
            {
                _context.PostLikes.Remove(like);
                _context.SaveChanges();
            }
        }

        public void Update(PostLike postLike)
        {
            _context.PostLikes.Update(postLike);
            _context.SaveChanges();
        }

        public PostLike? GetByUserAndPost(int userId, int postId)
        {
            return _context.PostLikes
                .FirstOrDefault(l => l.UserId == userId && l.PostId == postId);
        }

        public List<PostLike> GetByPostId(int postId)
        {
            return _context.PostLikes
                .Include(l => l.User)
                .Where(l => l.PostId == postId)
                .ToList();
        }

        public int CountByType(int postId, LikeType type)
        {
            return _context.PostLikes.Count(l => l.PostId == postId && l.Type == type);
        }
    }
}
