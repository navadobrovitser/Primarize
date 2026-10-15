using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Http;

namespace DTO.Candidates
{
    public class UpdateCandidateDto
    {
        public string Slogan { get; set; }
        public string Resume { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? ExplanationVideoUrl { get; set; }

        // שדות הקבצים לעדכון תמונה או סרטון
        public IFormFile? ImageFile { get; set; }
        public IFormFile? VideoFile { get; set; }
    }
}
