using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// /להוסיף מס פון
        /// </summary>
        public DateTime PartyJoinDate { get; set; }
        public List<PostLike> PostLikes { get; set; } = new List<PostLike>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
