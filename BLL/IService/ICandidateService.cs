using DTO.Candidates;


using System.Collections.Generic;

namespace BLL.IServices
{
    public interface ICandidateService
    {
        IEnumerable<CandidateListItemDto> GetAllCandidates();
        CandidateDto? GetCandidateById(string id); // שינוי מ-int ל-string
        CandidateDto CreateCandidate(CreateCandidateDto model);
        void UpdateCandidate(string id, UpdateCandidateDto model);
        void DeleteCandidate(string id);
    }
}