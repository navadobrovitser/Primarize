using BLL.IServices;
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

        //public bool CastVote(CastVoteDto model)
        //{
        //    if (_userVoteRepository.HasVoted(model.))
        //    {
        //        throw new InvalidOperationException("המשתמש כבר ביצע הצבעה בעבר.");
        //    }
        //    return true;
        //}

        public VoteStatusDto GetVoteStatus(int userId)
        {
            bool hasVoted = _userVoteRepository.HasVoted(userId);
            var vote = _userVoteRepository.GetByUserId(userId);

            return new VoteStatusDto
            {
                HasVoted = hasVoted,
                VoteDate = vote != null ? DateTime.Now : null
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