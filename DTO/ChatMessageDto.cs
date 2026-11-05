using System.ComponentModel.DataAnnotations;

namespace DTO // או בהתאם למרחב השמות בפרויקט שלך
{
    public class ChatMessageDto
    {
        [Required]
        [RegularExpression("^(user|assistant)$", ErrorMessage = "Role must be user or assistant")]
        public string Role { get; set; } = "";

        [Required(ErrorMessage = "Message text is required")]
        [MaxLength(2000)]
        public string Text { get; set; } = "";
    }
}