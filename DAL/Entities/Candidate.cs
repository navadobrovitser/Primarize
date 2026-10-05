using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class Candidate
    {
        [Key]
        public int CandidateId { get; set; }

       
        public int UserId { get; set; }
        public string Slogan { get; set; } = string.Empty;
        public string Resume { get; set; } = string.Empty; // קורות חיים

        // אחסון נתיב/קישור לתמונת הפרופיל ולסרטון ההסבר
        public string ProfileImageUrl { get; set; } = string.Empty;
        public string ? ExplanationVideoUrl { get; set; } = string.Empty;

        // Navigation Property לקישור למחלקת User
        public User User { get; set; } = null!;
        public List<Post> Posts { get; set; } = new List<Post>();
    }
}
