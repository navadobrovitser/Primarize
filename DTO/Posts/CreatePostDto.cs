using DTO.Validation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DTO.Posts
{
    public class CreatePostDto
    {
        [Required(ErrorMessage = "תוכן הפוסט הוא שדה חובה")]
        [StringLength(4000, ErrorMessage = "תוכן הפוסט אינו יכול לעבור 4000 תווים")]
        public string Content { get; set; }

        [Required(ErrorMessage = "סוג התוכן הוא שדה חובה")]
        public string ContentType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "מזהה מועמד חייב להיות מספר חיובי")]
        public int CandidateId { get; set; }


        [AllowedExtensions(".mp4", ".avi", ".mov")]
        [MaxFileSize(20)]
        public IFormFile? MediaFile { get; set; }
    }
}