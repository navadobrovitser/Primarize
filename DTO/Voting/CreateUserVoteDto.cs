using System.ComponentModel.DataAnnotations;

namespace DTO.Voting
{
    public class CreateUserVoteDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מזהה משתמש חייב להיות מספר חיובי")]
        public int UserId { get; set; }
    }
}