using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.IRepository
{
   
        public interface IAnonymousVoteRepository
        {
            AnonymousVote? GetById(int id);
            List<AnonymousVote> GetAll();
            void Add(AnonymousVote vote);
            void Delete(int id);
            void Update(AnonymousVote vote);

            // extra functions
            List<AnonymousVote> GetByCandidateId(int candidateId);
            int CountByCandidate(int candidateId);
            List<AnonymousVote> GetByRandomVoteId(string userRandomVoteId);
        }
    }
}
