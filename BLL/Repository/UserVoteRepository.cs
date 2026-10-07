using BLL.IRepository;
using DAL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace BLL.Repository
{
    public class UserVoteRepository : IUserVoteRepository
    {
        private readonly AppDbContext _context;

        public UserVoteRepository(AppDbContext context)
        {
            _context = context;
        }

        public UserVote? GetById(int id)
        {
            return _context.UserVotes
                .Include(v => v.User)
                .FirstOrDefault(v => v.UserVoteId == id);
        }

        public List<UserVote> GetAll()
        {
            return _context.UserVotes
                .Include(v => v.User)
                .ToList();
        }

        public void Add(UserVote userVote)
        {
            _context.UserVotes.Add(userVote);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var vote = _context.UserVotes.Find(id);
            if (vote != null)
            {
                _context.UserVotes.Remove(vote);
                _context.SaveChanges();
            }
        }

        public void Update(UserVote userVote)
        {
            _context.UserVotes.Update(userVote);
            _context.SaveChanges();
        }

        public UserVote? GetByUserId(int userId)
        {
            return _context.UserVotes.FirstOrDefault(v => v.UserId == userId);
        }

        public bool HasVoted(int userId)
        {
            return _context.UserVotes.Any(v => v.UserId == userId);
        }

        public int CountVotes()
        {
            return _context.UserVotes.Count();
        }
    }
}
