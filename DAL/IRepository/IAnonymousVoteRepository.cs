using DAL.Entities;
using System.Collections.Generic;

namespace DAL.IRepository
{
    public interface IAnonymousVoteRepository
    {
        AnonymousVote? GetById(int id);
        List<AnonymousVote> GetAll();
        void Add(AnonymousVote vote);
        void Delete(int id);
        void Update(AnonymousVote vote);
        List<AnonymousVote> GetByCandidateId(int candidateId);
        int CountByCandidate(int candidateId);
        List<AnonymousVote> GetByRandomVoteId(string userRandomVoteId);
    }
}