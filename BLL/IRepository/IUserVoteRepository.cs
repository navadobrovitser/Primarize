using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.IRepository
{
    public interface IUserVoteRepository
    {
        UserVote? GetById(int id);
        List<UserVote> GetAll();
        void Add(UserVote userVote);
        void Delete(int id);
        void Update(UserVote userVote);

        // extra functions
        UserVote? GetByUserId(int userId);
        bool HasVoted(int userId);
        int CountVotes();
    }
}
