using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Voting
{
    public class CreateAnonymousVoteDto
    {
        public int CandidateId { get; set; }
        public string UserRandomVoteId { get; set; } = string.Empty;
        public int Priority { get; set; }
    }
}
