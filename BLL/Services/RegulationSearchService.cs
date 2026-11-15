using BLL.IServices;
using DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class RegulationSearchService : IRegulationSearchService
    {
        private readonly AppDbContext _dbContext;
        private readonly IRegulationEmbeddingService _embeddingService;

        public RegulationSearchService(
            AppDbContext dbContext,
            IRegulationEmbeddingService embeddingService)
        {
            _dbContext = dbContext;
            _embeddingService = embeddingService;
        }

        // =====================================================
        // חיפוש סמנטי בתקנון המפורסם
        //
        // 1. מאתרים גרסה מפורסמת אחת.
        // 2. יוצרים Embedding לשאלת המשתמש.
        // 3. משווים לווקטורים של סעיפי התקנון.
        // 4. מחזירים את חמש התוצאות הטובות ביותר.
        // =====================================================

        public async Task<List<RegulationSearchResult>> SearchAsync(
            string question,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException(
                    "יש להזין שאלה.");
            }

            // =================================================
            // 1. בחירת הגרסה המפורסמת האחרונה
            //
            // לא מחפשים בטיוטות!
            // כל תוצאות החיפוש יהיו מאותה גרסה.
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
            // 2. שליפת הסעיפים של הגרסה שנבחרה בלבד
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
            // 3. יצירת Embedding לשאלה
            //
            // משתמשים ב-RETRIEVAL_QUERY,
            // ולא ב-RETRIEVAL_DOCUMENT.
            // =================================================

            float[] questionVector = await _embeddingService
                .GenerateQueryEmbeddingAsync(
                    question,
                    cancellationToken);

            // =================================================
            // 4. חישוב דמיון בין השאלה לכל סעיף
            // =================================================

            var results = new List<RegulationSearchResult>();

            foreach (var chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                float[]? documentVector =
                    JsonSerializer.Deserialize<float[]>(chunk.Embedding!);

                if (documentVector == null ||
                    documentVector.Length != questionVector.Length)
                {
                    throw new InvalidOperationException(
                        $"Embedding לא תקין בסעיף {chunk.SectionNumber}.");
                }

                double similarity = CosineSimilarity(
                    questionVector,
                    documentVector);

                results.Add(new RegulationSearchResult
                {
                    VersionId = version.VersionId,
                    SectionNumber = chunk.SectionNumber,
                    Content = chunk.Content,
                    Similarity = similarity
                });
            }

            // =================================================
            // 5. החזרת חמש התוצאות בעלות הדמיון הגבוה ביותר
            // =================================================

            return results
                .OrderByDescending(r => r.Similarity)
                .Take(5)
                .ToList();
        }

        // =====================================================
        // Cosine Similarity
        //
        // מחשב את הדמיון בין שני וקטורים.
        // ככל שהערך גבוה יותר, הכיוונים שלהם דומים יותר.
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