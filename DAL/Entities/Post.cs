using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Post
    {
        public int PostId { get; set; }

        // מפתח זר למועמד
        public int CandidateId { get; set; }

        // תוכן הפוסט (טקסט / נתיב לקובץ)
        public string Content { get; set; } = string.Empty;

        // סוג התוכן מתוך ה-Enum
        public PostContentType ContentType { get; set; }

        // תאריך פרסום הפוסט
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property לקשר מול מועמד
        public Candidate Candidate { get; set; } = null!;
        public List<PostLike> PostLikes { get; set; } = new List<PostLike>();
        public List<PostComment> PostComments { get; set; } = new List<PostComment>();
    }
}
