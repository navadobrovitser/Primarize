using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Voting
{
    public class VoteStatusDto
    {
        public bool HasVoted { get; set; }
        public DateTime? VoteDate { get; set; }
    }
}