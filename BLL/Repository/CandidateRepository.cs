using BLL.IRepository;
using BLL.Services;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Repository
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService; // הוספת שירות התמונות

        public CandidateRepository(AppDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
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

        // הוספת מועמד כולל טיפול בקובץ התמונה
        public Candidate Add(Candidate candidate, IFormFile? imageFile,IFormFile? videoFile)
        {
            if (imageFile != null)
            {
                // שמירת הקובץ בתיקייה וקבלת שם הקובץ הייחודי בלבד
                candidate.ProfileImageUrl = _imageService.Save(imageFile);
                
            }
            if (videoFile != null)
            {
                candidate.ExplanationVideoUrl = _imageService.SaveVideo(videoFile); // שמירת שם קובץ הסרטון
            }
            _context.Candidates.Add(candidate);
            _context.SaveChanges();
            return candidate;
        }

        public void Delete(int id)
        {
            var candidate = _context.Candidates.Find(id);
            if (candidate != null)
            {
                // מחיקת קובץ התמונה הפיזי מהתיקייה במידה וקיים
                if (!string.IsNullOrEmpty(candidate.ProfileImageUrl))
                {
                    _imageService.Delete(candidate.ProfileImageUrl);
                }

                _context.Candidates.Remove(candidate);
                _context.SaveChanges();
            }
        }

        public void Update(Candidate candidate, IFormFile? newImageFile)
        {
            var existingCandidate = _context.Candidates.AsNoTracking().FirstOrDefault(c => c.CandidateId == candidate.CandidateId);

            if (newImageFile != null)
            {
                // אם הועלתה תמונה חדשה, נמחק את הישנה ונסתיר/נשמור את החדשה
                if (existingCandidate != null && !string.IsNullOrEmpty(existingCandidate.ProfileImageUrl))
                {
                    _imageService.Delete(existingCandidate.ProfileImageUrl);
                }
                candidate.ProfileImageUrl = _imageService.Save(newImageFile);
            }
            else if (existingCandidate != null)
            {
                // שמירה על התמונה הקיימת אם לא הועלתה חדשה
                candidate.ProfileImageUrl = existingCandidate.ProfileImageUrl;
            }

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