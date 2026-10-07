using System;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        public string Content { get; set; } = string.Empty;

        public int UserId { get; set; }
        public int PostId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Post Post { get; set; } = null!;
    }
}