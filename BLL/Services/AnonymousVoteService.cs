using BLL.IServices;
using DAL.IRepository;
using System.Collections.Generic;

namespace BLL.Services
{
    public class AnonymousVoteService : IAnonymousVoteService
    {
        private readonly IAnonymousVoteRepository _anonymousVoteRepository;

        public AnonymousVoteService(IAnonymousVoteRepository anonymousVoteRepository)
        {
            _anonymousVoteRepository = anonymousVoteRepository;
        }

        public IEnumerable<object> GetAllAnonymousVotes()
        {
            return _anonymousVoteRepository.GetAll();
        }

        public void AddAnonymousVote(object model)
        {
            // הוספת הצבעה אנונימית
        }
    }
}