using BLL.IRepository;
using DAL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace BLL.Repository
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly AppDbContext _context;

        public CandidateRepository(AppDbContext context)
        {
            _context = context;
        }

        public Candidate? GetById(int id)
        {
            return _context.Candidates
                .Include(c => c.User)
                .Include(c => c.Posts)
                .FirstOrDefault(c => c.CandidateId == id);
        }

        public List<Candidate> GetAll()
        {
            return _context.Candidates
                .Include(c => c.User)
                .ToList();
        }

        public void Add(Candidate candidate)
        {
            _context.Candidates.Add(candidate);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var candidate = _context.Candidates.Find(id);
            if (candidate != null)
            {
                _context.Candidates.Remove(candidate);
                _context.SaveChanges();
            }
        }

        public void Update(Candidate candidate)
        {
            _context.Candidates.Update(candidate);
            _context.SaveChanges();
        }

        public Candidate? GetByUserId(int userId)
        {
            return _context.Candidates
                .Include(c => c.User)
                .Include(c => c.Posts)
                .FirstOrDefault(c => c.UserId == userId);
        }
    }
}

