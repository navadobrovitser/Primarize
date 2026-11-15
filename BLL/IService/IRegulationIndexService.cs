using System.Threading;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IRegulationIndexService
    {
        // יצירת Embeddings עבור מספר מוגבל של סעיפים.
        // מחזירה כמה וקטורים נוצרו בהפעלה הנוכחית.
        Task<int> GenerateEmbeddingsAsync(
            int versionId,
            int batchSize = 5,
            CancellationToken cancellationToken = default);
    }
}