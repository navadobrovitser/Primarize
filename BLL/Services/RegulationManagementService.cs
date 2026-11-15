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
        // מעתיקים את הסעיפים, אבל לא את ה-Embeddings.
        // כך אפשר לערוך את הטיוטה בלי להשפיע על הצ'אט.
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

            // מוודאים שמספר הגרסה עדיין לא קיים.
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

            // מאתרים את הגרסה המפורסמת האחרונה.
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

            // יוצרים גרסה חדשה במצב טיוטה.
            // התוכן מועתק, אך הווקטורים נשארים ריקים.
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
                        Embedding = null
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
        //
        // מאפשרת למסך הניהול להציג את כל הסעיפים
        // של גרסה מסוימת, כולל המבוא (0.0).
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
            // 4.2 יופיע לפני 4.10 ולא להפך.
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
        // אסור לשנות גרסה שכבר פורסמה.
        // אם התוכן משתנה, מבטלים את ה-Embedding
        // כדי שייווצר מחדש לפני הפרסום.
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

            // בדיקת מצב הגרסה.
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

            // איתור הסעיף שאותו המנהל רוצה לערוך.
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

            // אם לא בוצע שינוי אמיתי, אין צורך
            // למחוק את ה-Embedding הקיים.
            if (section.Content == updatedContent)
            {
                return;
            }

            section.Content = updatedContent;

            // הווקטור הישן כבר אינו מתאים לתוכן החדש.
            section.Embedding = null;

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
    }
}