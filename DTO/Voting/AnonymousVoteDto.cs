
namespace DTO.Voting
{
    public class AnonymousVoteDto
    {
        public int AnonymousVoteId { get; set; }
        public int CandidateId { get; set; }
        public string UserRandomVoteId { get; set; } = string.Empty;
        public int Priority { get; set; }
    }
}
