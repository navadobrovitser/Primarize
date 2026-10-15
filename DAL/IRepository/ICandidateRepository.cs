using DAL.Entities;
using System.Collections.Generic;

namespace DAL.IRepository
{
    public interface ICandidateRepository
    {
        Candidate? GetById(int id);
        List<Candidate> GetAll();
        Candidate Add(Candidate candidate);
        void Delete(int id);
        void Update(Candidate candidate);
        Candidate? GetByUserId(int userId);
    }
}
