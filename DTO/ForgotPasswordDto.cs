using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Security
{
    public class ForgotPasswordDto
    {
        public string IdNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
