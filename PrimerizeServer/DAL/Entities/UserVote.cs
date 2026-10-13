using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class UserVote
    {
        [Key]
        public int UserVoteId { get; set; }

        // מפתח זר למשתמש
        public int UserId { get; set; }

        public DateTime VoteDate { get; set; } = DateTime.UtcNow; // תאריך וזמן ההצבעה

        // Navigation Property לקשר מול משתמש
        public User User { get; set; } = null!;
    }
}
