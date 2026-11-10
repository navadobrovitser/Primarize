using DTO.Candidates;


using System.Collections.Generic;

namespace BLL.IServices
{
    public interface ICandidateService
    {
        CandidateEligibilityDto CheckEligibility(int userId);
        IEnumerable<CandidateListItemDto> GetAllCandidates();
        CandidateDto GetCandidateById(string id); // שינוי מ-int ל-string
        CandidateDto CreateCandidate(int currentUserId, CreateCandidateDto model);
        
        void DeleteCandidate(int currentUserId);
        void UpdateCandidate(int currentUserId, UpdateCandidateDto model);

    }
}