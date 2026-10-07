using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.IRepository
{
    public interface IPostRepository
    {
        Post? GetById(int id);
        List<Post> GetAll();
        void Add(Post post);
        void Delete(int id);
        void Update(Post post);

        // extra functions
        List<Post> GetByCandidateId(int candidateId);
        List<Post> GetByContentType(PostContentType type);
    }
}
