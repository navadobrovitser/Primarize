using System.Collections.Generic;
using DTO.Voting;

namespace BLL.IServices
{
    public interface IAnonymousVoteService
    {
        IEnumerable<AnonymousVoteDto> GetAllAnonymousVotes();
        AnonymousVoteDto? GetById(int id);
        void AddAnonymousVote(CreateAnonymousVoteDto model);
    }
}