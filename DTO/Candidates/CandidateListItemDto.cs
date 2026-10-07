using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Candidates
{
    public class CandidateListItemDto
    {
        public int CandidateId { get; set; }
        public string Name { get; set; }
        public string Slogan { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}
