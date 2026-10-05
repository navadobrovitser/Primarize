using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        // תוכן התגובה
        public string Content { get; set; } = string.Empty;

        // מפתח זר למשתמש
        public int UserId { get; set; }

        // מפתח זר לפוסט
        public int PostId { get; set; }

        // תאריך יצירת התגובה
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User User { get; set; } = null!;
        public Post Post { get; set; } = null!;
        public List<PostComment> PostComments { get; set; } = new List<PostComment>();
    }
}
