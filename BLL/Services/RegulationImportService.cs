using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BLL.IServices;
namespace BLL.Services
{
    public class RegulationImportService : IRegulationImportService
    {
        private readonly AppDbContext _dbContext;

        public RegulationImportService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<RegulationSection>> ReadSectionsAsync(
     string filePath)
        {
            // =====================================================
            // 1. בדיקה שקובץ התקנון קיים
            // =====================================================

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "קובץ התקנון לא נמצא.",
                    filePath);
            }

            // קריאת כל תוכן התקנון מהקובץ.
            string content = await File.ReadAllTextAsync(filePath);

            // =====================================================
            // 2. זיהוי כותרות הסעיפים
            //
            // לדוגמה:
            // 1.1 – הגשת בקשת הצטרפות
            // 2.3 – אימות משתמשים
            //
            // הביטוי מזהה מספר סעיף בתחילת שורה,
            // ואחריו מקף וכותרת.
            // =====================================================

            string pattern =
                @"(?m)^[ \t]*(\d+\.\d+)[ \t]*[–-][ \t]*[^\r\n]+";

            MatchCollection matches = Regex.Matches(content, pattern);

            if (matches.Count == 0)
            {
                throw new InvalidOperationException(
                    "לא נמצאו סעיפים בקובץ התקנון.");
            }

            // =====================================================
            // 3. חלוקת התקנון לסעיפים
            //
            // כל סעיף מתחיל בכותרת שלו ומסתיים
            // לפני הכותרת של הסעיף הבא.
            // =====================================================

            var sections = new List<RegulationSection>();
            // =====================================================
            // שמירת המבוא וההגדרות כיחידת מידע נפרדת
            //
            // המבוא נמצא לפני סעיף 1.1.
            // הוא חשוב לצ'אט ה-RAG ולכן נשמור גם אותו.
            //
            // 0.0 הוא מזהה פנימי בלבד ולא סעיף רשמי בתקנון.
            // =====================================================

            string introduction = content
                .Substring(0, matches[0].Index)
                .Trim();

            // מסירים את כותרת פרק 1 מסוף המבוא,
            // כדי שלא תיכלל בתוך תוכן ההגדרות.
            int firstChapterIndex = introduction.IndexOf(
                "פרק 1 –",
                StringComparison.Ordinal
            );

            if (firstChapterIndex >= 0)
            {
                introduction = introduction
                    .Substring(0, firstChapterIndex)
                    .Trim();
            }

            // מוסיפים את המבוא רק אם קיים בו תוכן.
            if (!string.IsNullOrWhiteSpace(introduction))
            {
                sections.Add(new RegulationSection
                {
                    SectionNumber = "0.0",
                    Content = introduction
                });
            }
            for (int i = 0; i < matches.Count; i++)
            {
                Match current = matches[i];

                int startIndex = current.Index;

                int endIndex = i + 1 < matches.Count
                    ? matches[i + 1].Index
                    : content.Length;

                string sectionText = content
                    .Substring(startIndex, endIndex - startIndex)
                    .Trim();

                // =================================================
                // 4. הסרת כותרת הפרק הבא
                //
                // לדוגמה, כותרת "פרק 2 – ..."
                // לא צריכה להיכלל בסעיף 1.5.
                // =================================================

                sectionText = Regex.Split(
                    sectionText,
                    @"(?m)^[ \t]*פרק[ \t]+\d+[ \t]*[–-]"
                )[0].Trim();

                sections.Add(new RegulationSection
                {
                    SectionNumber = current.Groups[1].Value,
                    Content = sectionText
                });
            }

            // =====================================================
            // 5. בדיקת תקינות הסעיפים
            // =====================================================

            if (sections.Any(s => string.IsNullOrWhiteSpace(s.Content)))
            {
                throw new InvalidOperationException(
                    "נמצא סעיף ללא תוכן.");
            }

            // מוודאים שאין שני סעיפים עם אותו מספר.
            var duplicates = sections
                .GroupBy(s => s.SectionNumber)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                throw new InvalidOperationException(
                    "נמצאו מספרי סעיפים כפולים: " +
                    string.Join(", ", duplicates));
            }

            return sections;
        }
        public async Task<int> ImportDraftAsync(
            string filePath,
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

            var sections = await ReadSectionsAsync(filePath);

            bool versionExists = await _dbContext.RegulationVersions
                .AnyAsync(
                    v => v.VersionNumber == versionNumber,
                    cancellationToken);

            if (versionExists)
            {
                throw new InvalidOperationException(
                    "כבר קיימת גרסת תקנון עם מספר זה.");
            }

            var version = new RegulationVersion
            {
                VersionNumber = versionNumber,
                Status = "Draft",
                PublishedAt = null,
                Chunks = sections.Select(s => new RegulationChunk
                {
                    SectionNumber = s.SectionNumber,
                    Content = s.Content,
                    Embedding = null
                }).ToList()
            };

            _dbContext.RegulationVersions.Add(version);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return version.VersionId;
        }
    }

    public class RegulationSection
    {
        public string SectionNumber { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}