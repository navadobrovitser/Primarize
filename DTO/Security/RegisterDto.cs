using System;
using System.ComponentModel.DataAnnotations;

namespace DTO.Security
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "כתובת אימייל היא שדה חובה")]
        [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "סיסמה היא שדה חובה")]
        [MinLength(8, ErrorMessage = "הסיסמה חייבת להכיל לפחות 8 תווים")]
        public string Password { get; set; } = "";
    }
}