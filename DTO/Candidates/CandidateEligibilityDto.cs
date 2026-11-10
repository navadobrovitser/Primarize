using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Candidates
{
    public class CandidateEligibilityDto
    {
        public bool Eligible { get; set; }
        public DateTime? PartyJoinDate { get; set; }
        public DateTime? EligibleFrom { get; set; }
        public bool AlreadyCandidate { get; set; }
    }
}
