using System.ComponentModel.DataAnnotations;

namespace DTO.Voting
{
    public class VoteChoiceDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מזהה מועמד חייב להיות מספר חיובי")]
        public int CandidateId { get; set; }

        [Range(1, 100, ErrorMessage = "דירוג עדיפות חייב להיות בין 1 ל-100")]
        public int Priority { get; set; } // דירוג העדיפות (1, 2, 3...)
    }
}