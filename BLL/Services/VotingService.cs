
using BLL.IServices;
using DAL;
using DAL.Entities;
using DAL.IRepository;
using DTO.Voting;
using BLL.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class VotingService : IVotingService
    {
        private readonly IUserVoteRepository _userVoteRepository;
        private readonly IAnonymousVoteRepository _anonymousVoteRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly AppDbContext _context;

        private readonly IUserRepository _userRepository;


        public VotingService(
            IUserVoteRepository userVoteRepository,
            IAnonymousVoteRepository anonymousVoteRepository,
            ICandidateRepository candidateRepository,
            IUserRepository userRepository,
            AppDbContext context)
        {
            _userVoteRepository = userVoteRepository;
            _anonymousVoteRepository = anonymousVoteRepository;
            _candidateRepository = candidateRepository;
            _userRepository = userRepository;
            _context = context;
        }

        // ביצוע הצבעה מלאה
        public bool CastVote(int userId, CastVoteDto model)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                throw new NotFoundException(
                    "המשתמש לא נמצא במערכת"
                );
            }

            if (!user.IsApproved)
            {
                throw new BusinessRuleException(
                    "רק חבר מפלגה מאושר רשאי להצביע"
                );
            }




            // 1. בדיקה שהמשתמש עדיין לא הצביע
            if (_userVoteRepository.HasVoted(userId))
            {
                throw new ConflictException(
                    "המשתמש כבר ביצע הצבעה בעבר.");
            }

            // 2. בדיקת מספר המועמדים
            if (model == null ||
                model.Rankings == null ||
                model.Rankings.Count < 1 ||
                model.Rankings.Count > 5)
            {
                throw new ArgumentException(
                    "יש לבחור בין מועמד אחד לחמישה מועמדים.");
            }

            // 3. בדיקה שאין מועמד שמופיע פעמיים
            var candidateIds = model.Rankings
                .Select(r => r.CandidateId)
                .ToList();

            if (candidateIds.Distinct().Count() != candidateIds.Count)
            {
                throw new ArgumentException(
                    "לא ניתן לבחור את אותו מועמד יותר מפעם אחת.");
            }

            // 4. בדיקה שהעדיפויות הן 1,2,3... ברצף
            var priorities = model.Rankings
                .Select(r => r.Priority)
                .OrderBy(p => p)
                .ToList();

            for (int i = 0; i < priorities.Count; i++)
            {
                if (priorities[i] != i + 1)
                {
                    throw new ArgumentException(
                        "סדר העדיפויות אינו תקין.");
                }
            }

            // 5. בדיקה שכל המועמדים קיימים
            var existingCandidateIds = _candidateRepository
                .GetAll()
                .Select(c => c.CandidateId)
                .ToHashSet();

            foreach (var candidateId in candidateIds)
            {
                if (!existingCandidateIds.Contains(candidateId))
                {
                    throw new ArgumentException(
                        "אחד המועמדים שנבחרו אינו קיים.");
                }
            }

            // 6. יצירת מזהה אקראי לפתק ההצבעה
            string randomVoteId = Guid.NewGuid().ToString("N");

            // 7. הכנת רשומת המשתמש
            var userVote = new UserVote
            {
                UserId = userId,
                VoteDate = DateTime.UtcNow.Date
            };

            // 8. הכנת כל הדירוגים האנונימיים
            var anonymousVotes = new List<AnonymousVote>();

            foreach (var ranking in model.Rankings)
            {
                anonymousVotes.Add(new AnonymousVote
                {
                    CandidateId = ranking.CandidateId,
                    Priority = ranking.Priority,
                    UserRandomVoteId = randomVoteId
                });
            }

            // 9. הכנת כל הרשומות לשמירה
            _userVoteRepository.AddWithoutSave(userVote);
            _anonymousVoteRepository.AddRange(anonymousVotes);

            // 10. שמירת כל הרשומות ביחד
            _context.SaveChanges();

            return true;
        }

        // בדיקת סטטוס ההצבעה של המשתמש
        public VoteStatusDto GetVoteStatus(int userId)
        {
            bool hasVoted = _userVoteRepository.HasVoted(userId);
            var vote = _userVoteRepository.GetByUserId(userId);

            if (hasVoted && vote == null)
            {
                throw new NotFoundException(
                    $"פרטי ההצבעה עבור משתמש {userId} לא נמצאו.");
            }

            return new VoteStatusDto
            {
                HasVoted = hasVoted,
                VoteDate = vote?.VoteDate
            };
        }

        // שליפת תוצאות הבחירות - נפתח בהמשך

        public ElectionResultsDto GetElectionResults()
        {
            // מספר המשתמשים שהצביעו
            int totalVoters = _userVoteRepository.CountVotes();

            // כל המועמדים
            var candidates = _candidateRepository.GetAll().ToList();

            // כל הדירוגים האנונימיים
            var votes = _anonymousVoteRepository.GetAll();

            // חישוב סדר המועמדים באמצעות IRV חוזר
            var rankingService = new ElectionRankingService();

            List<int> rankedCandidateIds =
                rankingService.CalculateRanking(candidates, votes);

            // הכנת התוצאות לפי הסדר שנקבע
            var results = new List<CandidateResultDto>();

            for (int i = 0; i < rankedCandidateIds.Count; i++)
            {
                int candidateId = rankedCandidateIds[i];

                var candidate = candidates.First(c =>
                    c.CandidateId == candidateId);

                results.Add(new CandidateResultDto
                {
                    CandidateId = candidateId,
                    Name = candidate.User.Name,
                    Position = i + 1,
                    TotalVotes = votes.Count(v =>
                        v.CandidateId == candidateId)
                });
            }

            return new ElectionResultsDto
            {
                TotalVoters = totalVoters,
                Results = results
            };
        }

    }
}
