
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO.Voting
{
    public class CastVoteDto
    {
        [Required(ErrorMessage = "חובה לבחור לפחות מועמד אחד")]
        [MinLength(1, ErrorMessage = "חובה לבחור לפחות מועמד אחד")]
        [MaxLength(5, ErrorMessage = "ניתן לבחור עד חמישה מועמדים")]
        public List<VoteChoiceDto> Rankings { get; set; } = new();
    }
}
