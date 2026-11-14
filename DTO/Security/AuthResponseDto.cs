using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Security
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
    }
}
