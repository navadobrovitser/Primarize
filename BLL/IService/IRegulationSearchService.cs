using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IRegulationSearchService
    {
        // מחפש עד חמישה קטעים רלוונטיים
        // בגרסת התקנון המפורסמת והפעילה.
        Task<List<RegulationSearchResult>> SearchAsync(
     string question,
     CancellationToken cancellationToken = default,
     string? currentQuestion = null);
    }

    public class RegulationSearchResult
    {
        public string SectionNumber { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public double Similarity { get; set; }
        public int VersionId { get; set; }
    }
}