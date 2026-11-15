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

        // נשמור וקטורים בעלי 768 ממדים.
        // בהמשך גם השאלות ייווצרו באותו מספר ממדים.
        private const int VectorDimensions = 768;

        public RegulationEmbeddingService(
            HttpClient httpClient,
            string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        // =====================================================
        // יצירת Embedding עבור סעיף מהתקנון
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
        // יצירת Embedding עבור שאלת משתמש
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
        // שליחת הטקסט ל-Gemini וקבלת הווקטור
        // =====================================================

        private async Task<float[]> GenerateEmbeddingAsync(
            string text,
            string taskType,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    "לא ניתן ליצור Embedding לטקסט ריק.");
            }

            string url =
                $"https://generativelanguage.googleapis.com/" +
                $"v1beta/models/{ModelName}:embedContent";

            // גוף הבקשה בהתאם למבנה של Gemini API.
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

            // מפתח ה-API מועבר בכותרת ולא בכתובת.
            request.Headers.Add(
                "x-goog-api-key",
                _apiKey);

            request.Content = JsonContent.Create(requestBody);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            // אם Gemini החזיר שגיאה, לא נשמור וקטור שגוי.
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken);

            using var json = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);

            // חילוץ רשימת המספרים מתשובת Gemini.
            JsonElement values = json.RootElement
                .GetProperty("embedding")
                .GetProperty("values");

            var vector = new List<float>();

            foreach (JsonElement value in values.EnumerateArray())
            {
                vector.Add(value.GetSingle());
            }

            if (vector.Count != VectorDimensions)
            {
                throw new InvalidOperationException(
                    "Gemini החזיר וקטור באורך לא צפוי.");
            }

            return vector.ToArray();
        }
    }
}