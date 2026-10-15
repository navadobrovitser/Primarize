using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Voting;
using System;
using System.Collections.Generic;

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

        public bool CastVote(int userId, CastVoteDto model)
        {
            if (_userVoteRepository.HasVoted(userId))
            {
                throw new InvalidOperationException("המשתמש כבר ביצע הצבעה בעבר.");
            }

            var newVote = new UserVote
            {
                UserId = userId,
                VoteDate = DateTime.UtcNow
            };

            _userVoteRepository.Add(newVote);
            return true;
        }

        public VoteStatusDto GetVoteStatus(int userId)
        {
            bool hasVoted = _userVoteRepository.HasVoted(userId);
            var vote = _userVoteRepository.GetByUserId(userId);

            return new VoteStatusDto
            {
                HasVoted = hasVoted,
                VoteDate = vote?.VoteDate
            };
        }

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