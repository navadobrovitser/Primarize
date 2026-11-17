
using BLL.IServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class RegulationEmbeddingService
        : IRegulationEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private const string ModelName = "gemini-embedding-001";

        // =====================================================
        // מספר הממדים של ה-Embeddings
        //
        // חשוב לשמור על 768, כי גם הווקטורים
        // הקיימים במסד הנתונים נוצרו בגודל הזה.
        // =====================================================

        private const int VectorDimensions = 768;

        public RegulationEmbeddingService(
            HttpClient httpClient,
            string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        // =====================================================
        // 1. יצירת Embedding עבור סעיף מהתקנון
        // =====================================================

        public Task<float[]> GenerateDocumentEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            return GenerateEmbeddingAsync(
                text,
                "RETRIEVAL_DOCUMENT",
                cancellationToken);
        }

        // =====================================================
        // 2. עבור שאלת משתמש  Embeddingיצירתמ
        // =====================================================

        public Task<float[]> GenerateQueryEmbeddingAsync(
            string question,
            CancellationToken cancellationToken = default)
        {
            return GenerateEmbeddingAsync(
                question,
                "RETRIEVAL_QUERY",
                cancellationToken);
        }

        // =====================================================
        // 3. שליחת הטקסט ל-Gemini וקבלת Embedding
        //
        // במקרה של תקלה בתקשורת עם Gemini,
        // נזרוק חריגה ייעודית שה-Controller מזהה.
        // =====================================================

        private async Task<float[]> GenerateEmbeddingAsync(
            string text,
            string taskType,
            CancellationToken cancellationToken)
        {
            // =============================================
            // בדיקת תקינות הטקסט
            // =============================================

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    "לא ניתן ליצור Embedding לטקסט ריק.");
            }

            // =============================================
            // כתובת השירות של Gemini
            // =============================================

            string url =
                "https://generativelanguage.googleapis.com/" +
                $"v1beta/models/{ModelName}:embedContent";

            // =============================================
            // בניית גוף הבקשה
            // =============================================

            var requestBody = new
            {
                model = $"models/{ModelName}",

                content = new
                {
                    parts = new[]
                    {
                        new { text }
                    }
                },

                taskType,

                outputDimensionality = VectorDimensions
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                url);

            // מפתח ה-API מועבר בכותרת הבקשה.
            request.Headers.Add(
                "x-goog-api-key",
                _apiKey);

            request.Content = JsonContent.Create(requestBody);

            // =============================================
            // 4. שליחת הבקשה ל-Gemini
            //
            // כאן מטפלים בשגיאות HTTP,
            // כולל 429 - חריגה ממכסת השימוש.
            // =============================================

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(
                    request,
                    cancellationToken);

                // בדיקה שהתקבלה תשובת HTTP תקינה.
                response.EnsureSuccessStatusCode();
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                // המשתמש ביטל את הבקשה.
                throw;
            }
            catch (HttpRequestException ex)
            {
                // שגיאת HTTP, כולל 429.
                System.Diagnostics.Debug.WriteLine(
                    $"RAG Embedding HTTP error: {ex}");

                throw new RegulationAiUnavailableException(
                    "שירות יצירת ה-Embeddings אינו זמין כרגע.",
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout בזמן התקשורת עם Gemini.
                System.Diagnostics.Debug.WriteLine(
                    $"RAG Embedding timeout: {ex}");

                throw new RegulationAiUnavailableException(
                    "שירות יצירת ה-Embeddings אינו זמין כרגע.",
                    ex);
            }

            // =============================================
            // 5. קריאת התשובה שהתקבלה
            //
            // מחלצים את הווקטור מתוך ה-JSON.
            // =============================================

            using (response)
            {
                using var stream = await response.Content
                    .ReadAsStreamAsync(cancellationToken);

                using var json = await JsonDocument.ParseAsync(
                    stream,
                    cancellationToken: cancellationToken);

                JsonElement values = json.RootElement
                    .GetProperty("embedding")
                    .GetProperty("values");

                var vector = new List<float>();

                foreach (JsonElement value in values.EnumerateArray())
                {
                    vector.Add(value.GetSingle());
                }

                // =========================================
                // 6. בדיקת מספר הממדים
                //
                // לא מחזירים וקטור באורך שגוי.
                // =========================================

                if (vector.Count != VectorDimensions)
                {
                    throw new InvalidOperationException(
                        "Gemini החזיר וקטור באורך לא צפוי.");
                }

                // =========================================
                // 7. החזרת ה-Embedding
                // =========================================

                return vector.ToArray();
            }
        }
    }
}
