using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Voting
{
    public class ElectionResultsDto
    {
        public List<CandidateResultDto> Results { get; set; }
        public int TotalVoters { get; set; }
    }
}
