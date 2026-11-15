using BLL.IServices;
using DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class RegulationSearchService : IRegulationSearchService
    {
        private readonly AppDbContext _dbContext;
        private readonly IRegulationEmbeddingService _embeddingService;

        private const int MaxResults = 5;

        public RegulationSearchService(
            AppDbContext dbContext,
            IRegulationEmbeddingService embeddingService)
        {
            _dbContext = dbContext;
            _embeddingService = embeddingService;
        }

        // =====================================================
        // חיפוש משולב בתקנון
        //
        // 1. בחירת הגרסה המפורסמת האחרונה.
        // 2. זיהוי מספרי סעיפים מפורשים בשאלה.
        // 3. איתור הסעיפים המבוקשים ישירות.
        // 4. השלמת התוצאות באמצעות חיפוש סמנטי.
        // 5. החזרת עד חמש תוצאות מאותה גרסה בלבד.
        // =====================================================

        public async Task<List<RegulationSearchResult>> SearchAsync(
      string question,
      CancellationToken cancellationToken = default,
      string? currentQuestion = null)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException(
                    "יש להזין שאלה.");
            }

            // =================================================
            // 1. בחירת הגרסה המפורסמת האחרונה
            //
            // טיוטות אינן משתתפות בחיפוש.
            // =================================================

            var version = await _dbContext.RegulationVersions
                .AsNoTracking()
                .Where(v => v.Status == "Published"
                            && v.PublishedAt != null)
                .OrderByDescending(v => v.PublishedAt)
                .ThenByDescending(v => v.VersionId)
                .FirstOrDefaultAsync(cancellationToken);

            if (version == null)
            {
                throw new InvalidOperationException(
                    "עדיין לא פורסמה גרסת תקנון.");
            }

            // =================================================
            // 2. שליפת סעיפי הגרסה המפורסמת בלבד
            // =================================================

            var chunks = await _dbContext.RegulationChunks
                .AsNoTracking()
                .Where(c => c.VersionId == version.VersionId)
                .ToListAsync(cancellationToken);

            if (chunks.Count == 0 ||
                chunks.Any(c => string.IsNullOrWhiteSpace(c.Embedding)))
            {
                throw new InvalidOperationException(
                    "מאגר הידע של התקנון עדיין אינו מוכן.");
            }

            // =================================================
            // 3. זיהוי מספרי סעיפים בשאלה
            //
            // לדוגמה:
            // "מה קובע סעיף 1.1?"
            // "מה ההבדל בין סעיף 4.16 לסעיף 5.2?"
            //
            // הביטוי מזהה מספר בפורמט X.Y
            // ואינו מזהה מספרים רגילים.
            // =================================================

            // =====================================================
            // זיהוי סעיפים מפורשים רק מתוך השאלה הנוכחית.
            //
            // question מכיל גם את ההיסטוריה לצורך חיפוש סמנטי.
            // currentQuestion מכיל רק את שאלת המשתמש החדשה.
            //
            // כך סעיפים שהוזכרו בשיחה קודמת לא יקבלו
            // עדיפות אוטומטית בחיפוש החדש.
            // =====================================================

            string textForExactSearch = currentQuestion ?? question;

            var requestedSectionNumbers = Regex.Matches(
                    textForExactSearch,
                    @"(?<![\d.])\d+\.\d+(?![\d.])"
                )
                .Select(m => m.Value)
                .Distinct()
                .ToList();

            // =================================================
            // 4. איתור הסעיפים שצוינו במפורש
            //
            // נותנים להם עדיפות על פני החיפוש הסמנטי.
            // לא מכניסים סעיפים שאינם קיימים.
            // =================================================

            var exactResults = new List<RegulationSearchResult>();

            foreach (string sectionNumber in requestedSectionNumbers)
            {
                var chunk = chunks.FirstOrDefault(
                    c => c.SectionNumber == sectionNumber);

                if (chunk == null)
                {
                    continue;
                }

                exactResults.Add(new RegulationSearchResult
                {
                    VersionId = version.VersionId,
                    SectionNumber = chunk.SectionNumber,
                    Content = chunk.Content,

                    // ערך עדיפות פנימי בלבד.
                    // אינו ציון דמיון סמנטי אמיתי.
                    Similarity = 2.0
                });

                if (exactResults.Count == MaxResults)
                {
                    return exactResults;
                }
            }

            // =================================================
            // 5. יצירת Embedding לשאלת המשתמש
            //
            // החיפוש הסמנטי ממשיך לפעול כרגיל.
            // =================================================

            float[] questionVector = await _embeddingService
                .GenerateQueryEmbeddingAsync(
                    question,
                    cancellationToken);

            // =================================================
            // 6. חישוב דמיון סמנטי
            //
            // לא מחשבים שוב סעיפים שכבר נמצאו
            // באמצעות מספר הסעיף.
            // =================================================

            var exactSectionNumbers = exactResults
                .Select(r => r.SectionNumber)
                .ToHashSet();

            var semanticResults =
                new List<RegulationSearchResult>();

            foreach (var chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (exactSectionNumbers.Contains(chunk.SectionNumber))
                {
                    continue;
                }

                float[]? documentVector =
                    JsonSerializer.Deserialize<float[]>(
                        chunk.Embedding!);

                if (documentVector == null ||
                    documentVector.Length != questionVector.Length)
                {
                    throw new InvalidOperationException(
                        $"Embedding לא תקין בסעיף {chunk.SectionNumber}.");
                }

                double similarity = CosineSimilarity(
                    questionVector,
                    documentVector);

                semanticResults.Add(new RegulationSearchResult
                {
                    VersionId = version.VersionId,
                    SectionNumber = chunk.SectionNumber,
                    Content = chunk.Content,
                    Similarity = similarity
                });
            }

            // =================================================
            // 7. שילוב התוצאות
            //
            // קודם סעיפים מפורשים, ואחריהם התוצאות
            // הסמנטיות הטובות ביותר.
            //
            // בכל מקרה מחזירים עד חמש תוצאות בלבד.
            // =================================================

            int remainingSlots = MaxResults - exactResults.Count;

            var bestSemanticResults = semanticResults
                .OrderByDescending(r => r.Similarity)
                .Take(remainingSlots)
                .ToList();

            return exactResults
                .Concat(bestSemanticResults)
                .ToList();
        }

        // =====================================================
        // חישוב Cosine Similarity
        //
        // מחשב את מידת הדמיון בין שני וקטורים.
        // =====================================================

        private static double CosineSimilarity(
            float[] first,
            float[] second)
        {
            double dotProduct = 0;
            double firstLengthSquared = 0;
            double secondLengthSquared = 0;

            for (int i = 0; i < first.Length; i++)
            {
                dotProduct += first[i] * second[i];

                firstLengthSquared += first[i] * first[i];
                secondLengthSquared += second[i] * second[i];
            }

            if (firstLengthSquared == 0 ||
                secondLengthSquared == 0)
            {
                return 0;
            }

            return dotProduct /
                (Math.Sqrt(firstLengthSquared) *
                 Math.Sqrt(secondLengthSquared));
        }
    }
}