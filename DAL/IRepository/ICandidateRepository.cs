using DAL.Entities;
using System.Collections.Generic;

namespace DAL.IRepository
{
    public interface ICandidateRepository
    {
        Candidate? GetByUserIdentityNumber(string idNumber);
        List<Candidate> GetAll();
        Candidate Add(Candidate candidate);
        void Delete(int id);
        void Update(Candidate candidate);
        Candidate? GetByUserId(int userId);
        Candidate? GetOwned(int candidateId, int userId);
    }
}
