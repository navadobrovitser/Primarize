using DAL.Entities;
using System.Collections.Generic;

namespace DAL.IRepository
{
    public interface IPostRepository
    {
        Post? GetById(int id);
        List<Post> GetAll();
        void Add(Post post);
        void Delete(int id);
        void Update(Post post);
        List<Post> GetByCandidateId(int candidateId);
        List<Post> GetByContentType(PostContentType type);
        Post? GetOwned(int id, int candidateId);
    }
}