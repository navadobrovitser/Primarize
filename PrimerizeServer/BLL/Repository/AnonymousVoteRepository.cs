using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLL.IRepository;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace BLL.Repository
{
    public class AnonymousVoteRepository : IAnonymousVoteRepository
    {
        private readonly AppDbContext _context;

        public AnonymousVoteRepository(AppDbContext context)
        {
            _context = context;
        }

        public AnonymousVote? GetById(int id)
        {
            return _context.AnonymousVotes
                .Include(v => v.Candidate)
                .FirstOrDefault(v => v.AnonymousVoteId == id);
        }

        public List<AnonymousVote> GetAll()
        {
            return _context.AnonymousVotes
                .Include(v => v.Candidate)
                .ToList();
        }

        public void Add(AnonymousVote vote)
        {
            _context.AnonymousVotes.Add(vote);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var vote = _context.AnonymousVotes.Find(id);
            if (vote != null)
            {
                _context.AnonymousVotes.Remove(vote);
                _context.SaveChanges();
            }
        }

        public void Update(AnonymousVote vote)
        {
            _context.AnonymousVotes.Update(vote);
            _context.SaveChanges();
        }

        public List<AnonymousVote> GetByCandidateId(int candidateId)
        {
            return _context.AnonymousVotes
                .Where(v => v.CandidateId == candidateId)
                .ToList();
        }

        public int CountByCandidate(int candidateId)
        {
            return _context.AnonymousVotes.Count(v => v.CandidateId == candidateId);
        }

        public List<AnonymousVote> GetByRandomVoteId(string userRandomVoteId)
        {
            return _context.AnonymousVotes
                .Where(v => v.UserRandomVoteId == userRandomVoteId)
                .OrderBy(v => v.Priority)
                .ToList();
        }
    }
}
