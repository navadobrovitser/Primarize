using DTO.Voting;

namespace BLL.IServices
{
    public interface IVotingService
    {
        bool CastVote(int userId, CastVoteDto model);
        VoteStatusDto GetVoteStatus(int userId);
        ElectionResultsDto GetElectionResults();
    }
}