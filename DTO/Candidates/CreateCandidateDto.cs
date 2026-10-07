using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Candidates
{
    public class CreateCandidateDto
    {
        public string Slogan { get; set; }
        public string Resume { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ExplanationVideoUrl { get; set; }
    }
}
