using System.ComponentModel.DataAnnotations;

namespace DTO.Comment
{
    public class UpdateCommentDto
    {
        [Required(ErrorMessage = "תוכן התגובה הוא שדה חובה")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "תוכן התגובה חייב להכיל בין תו 1 ל-1000 תווים")]
        public string Content { get; set; } = string.Empty; // תוכן התגובה המעודכן
    }
}