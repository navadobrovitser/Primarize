using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.IServices
{
    // הפעולות שמאפשרות למנהל האתר
    // לנהל טיוטות של התקנון.
    public interface IRegulationManagementService
    {
        // יצירת טיוטה חדשה כהעתק של גרסה מפורסמת.
        Task<int> CreateDraftFromPublishedAsync(
            string versionNumber,
            CancellationToken cancellationToken = default);

        // קבלת כל סעיפי גרסת תקנון לצורך הצגה ועריכה.
        Task<List<RegulationSectionEditDto>> GetSectionsAsync(
            int versionId,
            CancellationToken cancellationToken = default);

        // עדכון תוכן סעיף בטיוטה בלבד.
        Task UpdateSectionAsync(
            int versionId,
            string sectionNumber,
            string content,
            CancellationToken cancellationToken = default);
    }

    // מידע על סעיף שמוצג במסך הניהול.
    public class RegulationSectionEditDto
    {
        public string SectionNumber { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}