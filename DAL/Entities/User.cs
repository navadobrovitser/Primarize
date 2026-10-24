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
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string Phone { get; set; }
        public string Gender { get; set; }
        public DateTime PartyJoinDate { get; set; }
        public List<PostLike> PostLikes { get; set; } = new List<PostLike>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
