using BLL.IServices;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class RegulationManagementService
        : IRegulationManagementService
    {
        private readonly AppDbContext _dbContext;

        public RegulationManagementService(
            AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================
        // יצירת טיוטה חדשה מהתקנון המפורסם
        //
        // מעתיקים את הסעיפים ואת ה-Embeddings שלהם.
        // העתקת הווקטורים בטוחה מכיוון שהתוכן זהה.
        //
        // אם עורכים סעיף בהמשך, ה-Embedding שלו
        // מתבטל אוטומטית.
        // =====================================================

        public async Task<int> CreateDraftFromPublishedAsync(
            string versionNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(versionNumber))
            {
                throw new ArgumentException(
                    "יש להזין מספר גרסה.");
            }

            versionNumber = versionNumber.Trim();

            if (versionNumber.Length > 20)
            {
                throw new ArgumentException(
                    "מספר הגרסה ארוך מדי.");
            }

            // בדיקה שמספר הגרסה עדיין לא קיים.
            bool versionExists =
                await _dbContext.RegulationVersions
                    .AnyAsync(
                        v => v.VersionNumber == versionNumber,
                        cancellationToken);

            if (versionExists)
            {
                throw new InvalidOperationException(
                    "כבר קיימת גרסת תקנון עם מספר זה.");
            }

            // איתור הגרסה המפורסמת האחרונה.
            var publishedVersion =
                await _dbContext.RegulationVersions
                    .AsNoTracking()
                    .Include(v => v.Chunks)
                    .Where(v => v.Status == "Published")
                    .OrderByDescending(v => v.PublishedAt)
                    .ThenByDescending(v => v.VersionId)
                    .FirstOrDefaultAsync(cancellationToken);

            if (publishedVersion == null)
            {
                throw new InvalidOperationException(
                    "לא קיימת גרסת תקנון מפורסמת.");
            }

            // יצירת טיוטה חדשה.
            //
            // חשוב:
            // מכיוון שהתוכן מועתק ללא שינוי,
            // ניתן להשתמש באותו Embedding.
            //
            // אין כאן קריאה ל-Gemini.
            var draft = new RegulationVersion
            {
                VersionNumber = versionNumber,
                Status = "Draft",
                PublishedAt = null,

                Chunks = publishedVersion.Chunks
                    .Select(chunk => new RegulationChunk
                    {
                        SectionNumber = chunk.SectionNumber,
                        Content = chunk.Content,
                        Embedding = chunk.Embedding
                    })
                    .ToList()
            };

            _dbContext.RegulationVersions.Add(draft);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return draft.VersionId;
        }

        // =====================================================
        // שליפת סעיפי תקנון
        // =====================================================

        public async Task<List<RegulationSectionEditDto>>
            GetSectionsAsync(
                int versionId,
                CancellationToken cancellationToken = default)
        {
            bool versionExists =
                await _dbContext.RegulationVersions
                    .AsNoTracking()
                    .AnyAsync(
                        v => v.VersionId == versionId,
                        cancellationToken);

            if (!versionExists)
            {
                throw new InvalidOperationException(
                    "גרסת התקנון לא נמצאה.");
            }

            var sections = await _dbContext.RegulationChunks
                .AsNoTracking()
                .Where(c => c.VersionId == versionId)
                .Select(c => new RegulationSectionEditDto
                {
                    SectionNumber = c.SectionNumber,
                    Content = c.Content
                })
                .ToListAsync(cancellationToken);

            // מיון מספרי נכון:
            // 4.2 לפני 4.10.
            return sections
                .OrderBy(s => int.Parse(
                    s.SectionNumber.Split('.')[0]))
                .ThenBy(s => int.Parse(
                    s.SectionNumber.Split('.')[1]))
                .ToList();
        }

        // =====================================================
        // עדכון סעיף בטיוטה
        //
        // כאשר תוכן הסעיף משתנה, ה-Embedding
        // הישן מתבטל כדי למנוע שימוש בווקטור
        // שאינו מתאים לתוכן החדש.
        // =====================================================

        public async Task UpdateSectionAsync(
            int versionId,
            string sectionNumber,
            string content,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sectionNumber))
            {
                throw new ArgumentException(
                    "יש לציין מספר סעיף.");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(
                    "תוכן הסעיף אינו יכול להיות ריק.");
            }

            var version = await _dbContext.RegulationVersions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    v => v.VersionId == versionId,
                    cancellationToken);

            if (version == null)
            {
                throw new InvalidOperationException(
                    "גרסת התקנון לא נמצאה.");
            }

            if (version.Status != "Draft")
            {
                throw new InvalidOperationException(
                    "ניתן לערוך רק גרסת טיוטה.");
            }

            var section = await _dbContext.RegulationChunks
                .FirstOrDefaultAsync(
                    c => c.VersionId == versionId &&
                         c.SectionNumber == sectionNumber,
                    cancellationToken);

            if (section == null)
            {
                throw new InvalidOperationException(
                    "הסעיף המבוקש לא נמצא.");
            }

            string updatedContent = content.Trim();

            // אם התוכן לא השתנה, אין צורך לעדכן.
            if (section.Content == updatedContent)
            {
                return;
            }

            section.Content = updatedContent;

            // ביטול הווקטור הישן.
            section.Embedding = null;

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        // =====================================================
        // שימוש חוזר ב-Embeddings עבור טיוטה קיימת
        //
        // הפעולה משווה את תוכן הסעיפים בטיוטה
        // לתוכן הסעיפים בגרסה המפורסמת.
        //
        // רק כאשר התוכן זהה לחלוטין,
        // מעתיקים את ה-Embedding הקיים.
        //
        // אין קריאות ל-Gemini בפעולה זו.
        // =====================================================

        public async Task<int> ReuseEmbeddingsAsync(
            int versionId,
            CancellationToken cancellationToken = default)
        {
            // =============================================
            // 1. בדיקת הגרסה המבוקשת
            // =============================================

            var draft = await _dbContext.RegulationVersions
                .FirstOrDefaultAsync(
                    v => v.VersionId == versionId,
                    cancellationToken);

            if (draft == null)
            {
                throw new InvalidOperationException(
                    "גרסת התקנון לא נמצאה.");
            }

            if (draft.Status != "Draft")
            {
                throw new InvalidOperationException(
                    "ניתן להכין Embeddings רק לגרסת טיוטה.");
            }

            // =============================================
            // 2. איתור הגרסה המפורסמת
            // =============================================

            var publishedVersion =
                await _dbContext.RegulationVersions
                    .AsNoTracking()
                    .Where(v => v.Status == "Published")
                    .OrderByDescending(v => v.PublishedAt)
                    .ThenByDescending(v => v.VersionId)
                    .FirstOrDefaultAsync(cancellationToken);

            if (publishedVersion == null)
            {
                throw new InvalidOperationException(
                    "לא נמצאה גרסת תקנון מפורסמת.");
            }

            // =============================================
            // 3. טעינת סעיפי שתי הגרסאות
            // =============================================

            var publishedChunks =
                await _dbContext.RegulationChunks
                    .AsNoTracking()
                    .Where(c =>
                        c.VersionId == publishedVersion.VersionId)
                    .ToListAsync(cancellationToken);

            var draftChunks =
                await _dbContext.RegulationChunks
                    .Where(c => c.VersionId == versionId)
                    .ToListAsync(cancellationToken);

            // מילון לחיפוש מהיר לפי מספר סעיף.
            var publishedByNumber = publishedChunks
                .ToDictionary(c => c.SectionNumber);

            int reusedCount = 0;

            // =============================================
            // 4. העתקת וקטורים לסעיפים זהים בלבד
            // =============================================

            foreach (var draftChunk in draftChunks)
            {
                // אם כבר קיים Embedding, לא משנים אותו.
                if (!string.IsNullOrWhiteSpace(
                    draftChunk.Embedding))
                {
                    continue;
                }

                if (!publishedByNumber.TryGetValue(
                    draftChunk.SectionNumber,
                    out var publishedChunk))
                {
                    continue;
                }

                // רק התאמה מלאה של התוכן.
                if (!string.Equals(
                    draftChunk.Content,
                    publishedChunk.Content,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                    publishedChunk.Embedding))
                {
                    continue;
                }

                draftChunk.Embedding = publishedChunk.Embedding;
                reusedCount++;
            }

            // =============================================
            // 5. שמירת הווקטורים שהועתקו
            // =============================================

            if (reusedCount > 0)
            {
                await _dbContext.SaveChangesAsync(
                    cancellationToken);
            }

            return reusedCount;
        }
    }
}