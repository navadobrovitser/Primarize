
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

        public string Name { get; set; } = "";
        public string Slogan { get; set; } = "";
        public string? ProfileImageUrl { get; set; }

        public List<CandidateMatchSource> Sources { get; set; } = new();
    }

    public class CandidateMatchSource
    {
        public string SourceType { get; set; } = "";
        public int? PostId { get; set; }

        // exact / keyword / semantic
        public string MatchType { get; set; } = "";

        public string Quote { get; set; } = "";
    }
}
