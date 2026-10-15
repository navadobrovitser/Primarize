using Microsoft.AspNetCore.Http;

namespace DTO.Candidates
{
    public class CreateCandidateDto
    {
        public string Id { get; set; } // תעודת הזהות של המשתמש
        public string Slogan { get; set; }
        public string Resume { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? ExplanationVideoUrl { get; set; }

        // שדות הקבצים
        public IFormFile? ImageFile { get; set; }
        public IFormFile? VideoFile { get; set; }
    }
}