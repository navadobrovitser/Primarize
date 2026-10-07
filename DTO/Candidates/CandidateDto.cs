using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Candidates
{
    public class CandidateDto
    {
        public int CandidateId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } // מגיע מהמשתמש המקושר
        public string Slogan { get; set; }
        public string Resume { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ExplanationVideoUrl { get; set; }
    }
}
