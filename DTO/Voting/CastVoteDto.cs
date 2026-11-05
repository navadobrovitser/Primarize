using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO.Voting
{
    public class CastVoteDto
    {
        [Required(ErrorMessage = "חובה לשלוח לפחות דירוג אחד בפתק ההצבעה")]
        [MinLength(1, ErrorMessage = "פתק ההצבעה חייב להכיל לפחות בחירה אחת")]
        public List<VoteChoiceDto> Rankings { get; set; }
    }
}