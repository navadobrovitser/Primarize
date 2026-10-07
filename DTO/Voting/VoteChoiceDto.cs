using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Voting
{
    public class VoteChoiceDto
    {
        public int CandidateId { get; set; }
        public int Priority { get; set; } // דירוג העדיפות (1, 2, 3...)
    }
}
