using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.Security
{
    
        public class RegisterDto
        {
            [Required, EmailAddress]
            public string Email { get; set; } = "";

            [Required, MinLength(8)]
            public string Password { get; set; } = "";
        }
    }

