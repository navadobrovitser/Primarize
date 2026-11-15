using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Security
{
    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}
