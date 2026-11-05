using System;
using System.ComponentModel.DataAnnotations;

namespace DTO.Security
{
    public class LoginDto
    {
        [Required(ErrorMessage = "כתובת אימייל היא שדה חובה")]
        [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "סיסמה היא שדה חובה")]
        public string Password { get; set; } = "";
    }
}