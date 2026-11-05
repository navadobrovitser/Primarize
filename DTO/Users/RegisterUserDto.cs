using System.ComponentModel.DataAnnotations;

namespace DTO.Users
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "תעודת זהות היא שדה חובה")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "תעודת זהות חייבת להכיל בדיוק 9 ספרות")]
        public string Id { get; set; }

        [Required(ErrorMessage = "שם הוא שדה חובה")]
        [StringLength(50, ErrorMessage = "שם אינו יכול לעבור 50 תווים")]
        public string Name { get; set; }

        [Required(ErrorMessage = "כתובת אימייל היא שדה חובה")]
        [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה")]
        public string Email { get; set; }

        [Required(ErrorMessage = "סיסמה היא שדה חובה")]
        [MinLength(8, ErrorMessage = "הסיסמה חייבת להכיל לפחות 8 תווים")]
        public string Password { get; set; }

        [Required(ErrorMessage = "מספר טלפון הוא שדה חובה")]
        [Phone(ErrorMessage = "מספר הטלפון אינו תקין")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "מגדר הוא שדה חובה")]
        public string Gender { get; set; }
    }
}