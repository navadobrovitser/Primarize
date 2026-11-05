using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Voting;
using System;
using System.Collections.Generic;
using BLL.Exceptions; // הוספת ה-using לחריגות שלנו

namespace BLL.Services
{
    public class VotingService : IVotingService
    {
        private readonly IUserVoteRepository _userVoteRepository;
        private readonly ICandidateRepository _candidateRepository;

        public VotingService(IUserVoteRepository userVoteRepository, ICandidateRepository candidateRepository)
        {
            _userVoteRepository = userVoteRepository;
            _candidateRepository = candidateRepository;
        }

        // ביצוע הצבעה - זריקת ConflictException במידה והמשתמש כבר הצביע
        public bool CastVote(int userId, CastVoteDto model)
        {
            if (_userVoteRepository.HasVoted(userId))
            {
                throw new ConflictException("המשתמש כבר ביצע הצבעה בעבר.");
            }

            var newVote = new UserVote
            {
                UserId = userId,
                VoteDate = DateTime.UtcNow
            };

            _userVoteRepository.Add(newVote);
            return true;
        }

        // בדיקת סטטוס הצבעה
        public VoteStatusDto GetVoteStatus(int userId)
        {
            bool hasVoted = _userVoteRepository.HasVoted(userId);
            var vote = _userVoteRepository.GetByUserId(userId);

            if (hasVoted && vote == null)
            {
                throw new NotFoundException($"פרטי ההצבעה עבור משתמש {userId} לא נמצאו.");
            }

            return new VoteStatusDto
            {
                HasVoted = hasVoted,
                VoteDate = vote?.VoteDate
            };
        }

        // שליפת תוצאות הבחירות
        public ElectionResultsDto GetElectionResults()
        {
            int totalVotes = _userVoteRepository.CountVotes();

            return new ElectionResultsDto
            {
                TotalVoters = totalVotes,
                Results = new List<CandidateResultDto>()
            };
        }
    }
}