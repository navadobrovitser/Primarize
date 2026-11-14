
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class AnonymousVote
    {
        [Key]
        public int AnonymousVoteId { get; set; }

        public int CandidateId { get; set; }

        public int Priority { get; set; }

        // מזהה אקראי המשותף לכל הדירוגים באותו פתק
        public string UserRandomVoteId { get; set; } = string.Empty;

        public Candidate Candidate { get; set; } = null!;
    }
}
