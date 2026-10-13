using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Voting
{
    public class CastVoteDto
    {
        public List<VoteChoiceDto> Rankings { get; set; }
    }
}