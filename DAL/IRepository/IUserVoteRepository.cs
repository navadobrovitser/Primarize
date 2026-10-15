using DAL.Entities;


using System.Collections.Generic;

namespace DAL.IRepository
{
    public interface IUserVoteRepository
    {
        UserVote? GetById(int id);
        List<UserVote> GetAll();
        void Add(UserVote userVote);
        void Delete(int id);
        void Update(UserVote userVote);
        UserVote? GetByUserId(int userId);
        bool HasVoted(int userId);
        int CountVotes();
    }
}