using DTO.Voting;

namespace BLL.IServices
{
    public interface IVotingService
    {
        bool CastVote(CastVoteDto model);
        VoteStatusDto GetVoteStatus(int userId);
        ElectionResultsDto GetElectionResults();
    }
}