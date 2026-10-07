using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Voting
{
    public class CandidateResultDto
    {
        public int CandidateId { get; set; }
        public string Name { get; set; }
        public int TotalVotes { get; set; }
    }
}