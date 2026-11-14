
using System.ComponentModel.DataAnnotations;

namespace DTO.Voting
{
    public class VoteChoiceDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מזהה מועמד לא תקין")]
        public int CandidateId { get; set; }

        [Range(1, 5, ErrorMessage = "העדיפות חייבת להיות בין 1 ל-5")]
        public int Priority { get; set; }
    }
}
