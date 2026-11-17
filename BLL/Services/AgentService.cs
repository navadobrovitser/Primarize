
using DTO;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using BLL.IServices;

namespace BLL.Services
{
    public class AgentService : IAgentService
    {
        // =====================================================
        // 1. System Prompt
        //
        // הגדרת תפקיד סוכן ה-AI והמבנה האחיד
        // לעיבוד וניסוח קורות החיים של המועמדים.
        // =====================================================

        private const string SystemPrompt = """
            אתה עוזר מקצועי לניסוח ועיצוב קורות חיים. תפקידך לקחת טקסט חופשי מהמועמד ולעצב אותו מחדש בתבנית אחידה, נקייה ומסודרת הכוללת את תתי-הנושאים הבאים:
            1. פרטים אישיים וסטטוס: שם מלא, דרכי התקשרות וסטטוס נוכחי.
            2. שאיפות מקצועיות: הכיוון של המועמד לטווח הקצר והארוך והאתגרים שהוא מחפש.
            3. אג'נדה ותפיסת עולם: הערכים המרכזיים והגישה המקצועית שמנחים אותו.
            4. הבטחות ותרומה מרכזית: הערך המוסף והייחודיות שהמוועמד מביא איתו לארגון.
            5. ניסיון מקצועי והשכלה: רשימת תפקידים, פרויקטים והישגים בולטים.
            6. כישורים ומיומנויות: כלים טכנולוגיים, שפות ומיומנויות נדרשות.
            הקפד על ניסוח מקצועי ותבנית אחידה ועקבית לכל מועמד.
            """;

        private readonly IChatClient _chatClient;

        // Dependency Injection
        public AgentService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        // =====================================================
        // 2. בניית היסטוריית השיחה
        //
        // שילוב הוראות המערכת עם הודעות המשתמש והסוכן.
        // שמירה על תפקידי ההודעות לצורך רציפות השיחה.
        // =====================================================

        private List<ChatMessage> BuildMessages(
            List<ChatMessageDto> history)
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, SystemPrompt)
            };

            foreach (var m in history)
            {
                var role = m.Role == "assistant"
                    ? ChatRole.Assistant
                    : ChatRole.User;

                messages.Add(new ChatMessage(role, m.Text));
            }

            return messages;
        }

        // =====================================================
        // 3. קבלת תשובה מלאה מה-AI
        //
        // שליחת היסטוריית השיחה למודל
        // והחזרת התשובה לאחר השלמת העיבוד.
        // =====================================================

        public async Task<string> AskAsync(
            List<ChatMessageDto> history,
            CancellationToken ct)
        {
            var response = await _chatClient.GetResponseAsync(
                BuildMessages(history),
                cancellationToken: ct);

            return response.Text;
        }

        // =====================================================
        // 4. Streaming Response
        //
        // קבלת התשובה באופן הדרגתי והעברתה בחלקים.
        // מאפשר הצגת טקסט תוך כדי יצירתו על ידי המודל.
        // =====================================================

        public async IAsyncEnumerable<string> AskStreamingAsync(
            List<ChatMessageDto> history,
            [EnumeratorCancellation] CancellationToken ct)
        {
            var updates = _chatClient.GetStreamingResponseAsync(
                BuildMessages(history),
                cancellationToken: ct);

            await foreach (var update in updates)
            {
                if (!string.IsNullOrEmpty(update.Text))
                    yield return update.Text;
            }
        }
    }
}
