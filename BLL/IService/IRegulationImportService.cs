using BLL.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IRegulationImportService
    {
        Task<List<RegulationSection>> ReadSectionsAsync(
            string filePath);

        Task<int> ImportDraftAsync(
            string filePath,
            string versionNumber,
            CancellationToken cancellationToken = default);
    }
}