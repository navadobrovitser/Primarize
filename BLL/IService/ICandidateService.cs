using DTO.Candidates;


using System.Collections.Generic;

namespace BLL.IServices
{
    public interface ICandidateService
    {
        IEnumerable<CandidateListItemDto> GetAllCandidates();
        CandidateDto? GetCandidateById(int id);
        CandidateDto CreateCandidate(CreateCandidateDto model, int userId);
        void UpdateCandidate(int id, CreateCandidateDto model);
        void DeleteCandidate(int id);
    }
}