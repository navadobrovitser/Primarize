using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Users
{
    // שלב 1: שליחת ת"ז וסיסמה לקבלת קוד למייל
    public class LoginRequestDto
    {
        public string IdNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // שלב 2: אימות הקוד הסופי יחד עם הת"ז
    public class VerifyOtpDto
    {
        public string IdNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
