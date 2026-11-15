using System.Threading;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IRegulationEmbeddingService
    {
        // יצירת וקטור עבור סעיף בתקנון.
        Task<float[]> GenerateDocumentEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default);

        // יצירת וקטור עבור שאלה של משתמש.
        Task<float[]> GenerateQueryEmbeddingAsync(
            string question,
            CancellationToken cancellationToken = default);
    }
}