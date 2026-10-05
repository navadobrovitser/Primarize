using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class PostLike
    {
        [Key]
        public int PostLikeId { get; set; }

        // מפתחות זרים
        public int PostId { get; set; }
        public int UserId { get; set; }

        // סוג הלייק (Like / Dislike)
        public LikeType Type { get; set; }

        // Navigation Properties
        public Post Post { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
