using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class AnonymousVote
    {
        [Key]
        public int AnonymousVoteId { get; set; }

        // מפתח זר למועמד
        public int CandidateId { get; set; }

        public int Priority { get; set; } // דרגת עדיפות
        public DateTime VoteTime { get; set; } = DateTime.UtcNow; // זמן ההצבעה

        // מזהה ההצבעה האקראי של המשתמש (משלב מזהה אקראי למשתמש)
        public string UserRandomVoteId { get; set; } = string.Empty;

        // Navigation Property לקשר מול מועמד
        public Candidate Candidate { get; set; } = null!;
    }
}
