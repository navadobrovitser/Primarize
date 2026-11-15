using BLL.IServices;
using DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class RegulationIndexService : IRegulationIndexService
    {
        private readonly AppDbContext _dbContext;
        private readonly IRegulationEmbeddingService _embeddingService;

        public RegulationIndexService(
            AppDbContext dbContext,
            IRegulationEmbeddingService embeddingService)
        {
            _dbContext = dbContext;
            _embeddingService = embeddingService;
        }

        // =====================================================
        // יצירת Embeddings במנות עבור גרסת תקנון
        //
        // מעבדים רק סעיפים שעדיין אין להם Embedding.
        // כך ניתן להפעיל את הפעולה מספר פעמים
        // ולהמשיך מהמקום שבו עצרנו.
        // =====================================================

        public async Task<int> GenerateEmbeddingsAsync(
            int versionId,
            int batchSize = 5,
            CancellationToken cancellationToken = default)
        {
            // =================================================
            // 1. בדיקת גודל המנה
            // =================================================

            if (batchSize < 1 || batchSize > 10)
            {
                throw new ArgumentException(
                    "גודל המנה חייב להיות בין 1 ל-10.");
            }

            // =================================================
            // 2. בדיקה שהגרסה קיימת ונמצאת במצב Draft
            // =================================================

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
                    "ניתן ליצור Embeddings רק עבור טיוטת תקנון.");
            }

            // =================================================
            // 3. שליפת סעיפים שעדיין אין להם Embedding
            //
            // Take מגביל את מספר הסעיפים למנה המבוקשת.
            // =================================================

            var chunks = await _dbContext.RegulationChunks
                .Where(c =>
                    c.VersionId == versionId &&
                    c.Embedding == null)
                .OrderBy(c => c.ChunkId)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            int generatedCount = 0;

            // =================================================
            // 4. יצירת Embedding ושמירתו לכל סעיף
            // =================================================

            foreach (var chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // יצירת וקטור באמצעות Gemini.
                float[] vector = await _embeddingService
                    .GenerateDocumentEmbeddingAsync(
                        chunk.Content,
                        cancellationToken);

                // שמירת הווקטור כ-JSON במסד הנתונים.
                chunk.Embedding = JsonSerializer.Serialize(vector);

                // שמירה לאחר כל סעיף כדי לא לאבד התקדמות.
                await _dbContext.SaveChangesAsync(
                    cancellationToken);

                generatedCount++;
            }

            return generatedCount;
        }
    }
}