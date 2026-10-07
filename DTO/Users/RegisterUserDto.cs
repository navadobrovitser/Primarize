using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Users
{
    public class RegisterUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
    }
}
