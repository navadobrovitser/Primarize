
using DTO.Candidates;
using System.Collections.Generic;

namespace BLL.IServices
{
    public interface ICandidateService
    {
        CandidateEligibilityDto CheckEligibility(int userId);

        IEnumerable<CandidateListItemDto> GetAllCandidates();

        CandidateDto GetCandidateById(string id);

        CandidateDto CreateCandidate(
            int currentUserId,
            CreateCandidateDto model
        );

        IEnumerable<CandidateDto> GetCandidatesForMatch();

        void DeleteCandidate(int currentUserId);

        void UpdateCandidate(
            int currentUserId,
            UpdateCandidateDto model
        );

        // הפונקציה שלך - שליפת מועמד לפי CandidateId
        CandidateDto GetCandidateByCandidateId(int candidateId);

        // הפונקציה של החברה - שליפת המועמד של המשתמש המחובר
        CandidateDto GetMyCandidate(int currentUserId);
    }
}
