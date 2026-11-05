using System.ComponentModel.DataAnnotations;

namespace DTO.Comment
{
    public class CreateCommentDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מזהה פוסט חייב להיות מספר חיובי")]
        public int PostId { get; set; } // מזהה הפוסט אליו משתייכת התגובה

        [Required(ErrorMessage = "תוכן התגובה הוא שדה חובה")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "תוכן התגובה חייב להכיל בין תו 1 ל-1000 תווים")]
        public string Content { get; set; } = string.Empty; // תוכן התגובה
    }
}