
using BLL.IServices;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BLL.Services
{
    public class CandidateMatchService : ICandidateMatchService
    {
        private readonly IChatClient _chatClient;
        private readonly ICandidateService _candidateService;

        public CandidateMatchService(
            IChatClient chatClient,
            ICandidateService candidateService)
        {
            _chatClient = chatClient;
            _candidateService = candidateService;
        }

        public async Task<CandidateMatchResponse> MatchAsync(
            string query,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("יש להזין בקשת חיפוש.");

            // מועמדים אמיתיים ממסד הנתונים
            var candidates = _candidateService
      .GetCandidatesForMatch()
      .ToList();

            if (candidates.Count == 0)
                return new CandidateMatchResponse();
            var candidateData = candidates.Select(c => new
            {
                c.CandidateId,
                c.Name,
                c.Slogan,
                c.Resume
            });


            var prompt = $$"""
    אתה מנוע חיפוש סמנטי של מועמדים.

    המשימה:
    לזהות אילו מועמדים עוסקים בנושאים
    שהמשתמש מחפש, על בסיס המידע שסופק בלבד.

    כללים:
    - אין להמציא פרטים או עמדות.
    - אין להשתמש במידע חיצוני.
    - אין לתת המלצת הצבעה.
    - החזר רק מועמדים שיש להם קשר מתועד לבקשה.
    - השתמש רק במזהי מועמדים שקיימים ברשימה.
    - החזר JSON תקין בלבד, ללא Markdown.

    בקשת המשתמש:
    {{query}}

    נתוני המועמדים:
    {{JsonSerializer.Serialize(candidateData)}}

    פורמט התשובה:
    {
      "detectedTopics": ["נושא"],
      "results": [
        {
          "candidateId": 1,
          "score": 80,
          "explanation": "הסבר קצר בעברית",
          "matchedTopics": ["נושא"]
        }
      ]
    }
    """;


            var messages = new List<ChatMessage>
            {
                new(ChatRole.User, prompt)
            };

            var response = await _chatClient.GetResponseAsync(
                messages,
                cancellationToken: ct
            );

            var raw = response.Text ?? "";

            // הסרת סימוני Markdown אם המודל הוסיף אותם
            raw = Regex.Replace(
                raw,
                @"^```(?:json)?\s*|\s*```$",
                "",
                RegexOptions.IgnoreCase
            ).Trim();

            var result = JsonSerializer.Deserialize<CandidateMatchResponse>(
                raw,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new CandidateMatchResponse();

            // אין להחזיר מזהים שלא קיימים במסד הנתונים
            var validIds = candidates
                .Select(c => c.CandidateId)
                .ToHashSet();

          

            result.Results = result.Results
    .Where(r => validIds.Contains(r.CandidateId))
    .GroupBy(r => r.CandidateId)
    .Select(g => g.First())
    .Select(r =>
    {
        r.Score = Math.Clamp(r.Score, 0, 100);
        return r;
    })
    .OrderByDescending(r => r.Score)
    .ToList();

            // השלמת נתוני המועמדים
            var candidateLookup = candidates.ToDictionary(
                c => c.CandidateId
            );

            foreach (var item in result.Results)
            {
                if (candidateLookup.TryGetValue(
                    item.CandidateId,
                    out var candidate))
                {
                    item.Name = candidate.Name;
                    item.Slogan = candidate.Slogan;
                    item.ProfileImageUrl = candidate.ProfileImageUrl;
                }
            }

            return result;
        }
    }
}
