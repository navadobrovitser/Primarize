using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        public int CandidateId { get; set; }
        public string Content { get; set; } = string.Empty;
        public PostContentType ContentType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Candidate Candidate { get; set; } = null!;
        public List<PostLike> PostLikes { get; set; } = new List<PostLike>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}