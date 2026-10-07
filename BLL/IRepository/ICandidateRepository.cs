using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.IRepository
{
    public interface ICandidateRepository
    {
        Candidate? GetById(int id);
        List<Candidate> GetAll();
        void Add(Candidate candidate);
        void Delete(int id);
        void Update(Candidate candidate);

        // extra functions
        Candidate? GetByUserId(int userId);
    }
}
