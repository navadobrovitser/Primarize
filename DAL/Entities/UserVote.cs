
using System;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class UserVote
    {
        [Key]
        public int UserVoteId { get; set; }

        public int UserId { get; set; }

        // תאריך בלבד, ללא שעה מדויקת
        public DateTime VoteDate { get; set; } = DateTime.UtcNow.Date;

        public User User { get; set; } = null!;
    }
}
