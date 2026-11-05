using System.ComponentModel.DataAnnotations;

namespace DTO.Voting
{
    public class CreateAnonymousVoteDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מזהה מועמד חייב להיות מספר חיובי")]
        public int CandidateId { get; set; }

        [Required(ErrorMessage = "מזהה הצבעה אנונימי הוא שדה חובה")]
        public string UserRandomVoteId { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "דירוג עדיפות חייב להיות בין 1 ל-100")]
        public int Priority { get; set; }
    }
}