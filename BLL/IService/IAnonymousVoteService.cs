using System.Collections.Generic;

namespace BLL.IServices
{
    public interface IAnonymousVoteService
    {
        IEnumerable<object> GetAllAnonymousVotes();
        void AddAnonymousVote(object model);
    }
}