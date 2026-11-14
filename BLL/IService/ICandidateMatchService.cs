
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface ICandidateMatchService
    {
        Task<CandidateMatchResponse> MatchAsync(
            string query,
            CancellationToken ct);
    }

    public class CandidateMatchResponse
    {
        public List<string> DetectedTopics { get; set; } = new();

        public List<CandidateMatchItem> Results { get; set; } = new();
    }

    public class CandidateMatchItem
    {
        public int CandidateId { get; set; }

        public int Score { get; set; }

        public string Explanation { get; set; } = "";

        public List<string> MatchedTopics { get; set; } = new();

        // פרטי המועמד שנציג ב-React
        public string Name { get; set; } = "";

        public string Slogan { get; set; } = "";

        public string? ProfileImageUrl { get; set; }
    }
}
