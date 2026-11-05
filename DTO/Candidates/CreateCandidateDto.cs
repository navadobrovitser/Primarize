using DTO.Validation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DTO.Candidates
{
    public class CreateCandidateDto
    {
        [Required(ErrorMessage = "תעודת זהות היא שדה חובה")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "תעודת זהות חייבת להכיל בדיוק 9 ספרות")]
        public string Id { get; set; }

        [Required(ErrorMessage = "סלוגן הוא שדה חובה")]
        [StringLength(100, ErrorMessage = "סלוגן לא יכול לעבור 100 תווים")]
        public string Slogan { get; set; }

        [Required(ErrorMessage = "קורות חיים הם שדה חובה")]
        [StringLength(2000, ErrorMessage = "קורות חיים לא יכולים לעבור 2000 תווים")]
        public string Resume { get; set; }

        public string? ProfileImageUrl { get; set; }
        public string? ExplanationVideoUrl { get; set; }

        // שדות הקבצים להעלאה
        [AllowedExtensions(".jpg", ".jpeg", ".png", ".webp")]
        [MaxFileSize(5)]
        public IFormFile? ImageFile { get; set; }
        [AllowedExtensions(".mp4", ".avi", ".mov")]
        [MaxFileSize(20)]
        public IFormFile? VideoFile { get; set; }
    }
}