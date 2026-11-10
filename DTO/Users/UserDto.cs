using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Users
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Id { get; set; } // תעודת זהות
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // ---> הוספת שדה הסיסמה
        public string Phone { get; set; }
        public string Gender { get; set; }
        public DateTime PartyJoinDate { get; set; }
    }
}
