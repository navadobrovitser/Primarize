using DAL.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.IRepository
{
    public interface ICandidateRepository
    {
        Candidate? GetById(int id);
        List<Candidate> GetAll();
       Candidate Add(Candidate candidate,  IFormFile? imageFile,IFormFile? videoFile);
        void Delete(int id);
        void Update(Candidate candidate, IFormFile? newImageFile);

        // extra functions
        Candidate? GetByUserId(int userId);
    }
}
