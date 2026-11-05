using System.ComponentModel.DataAnnotations;

namespace DTO.Likes
{
    public class SetPostLikeDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מזהה פוסט חייב להיות מספר חיובי")]
        public int PostId { get; set; }

        [Required(ErrorMessage = "סוג הדירוג/הלייק הוא שדה חובה")]
        public string Type { get; set; } = string.Empty;
    }
}