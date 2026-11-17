
using DTO.Voting;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

namespace API.Services
{
    public class ElectionStateService
    {
        private readonly object _lock = new();
        private readonly string _filePath;
        private ElectionState _state;

        // =====================================================
        // 1. אתחול וטעינת מצב הבחירות
        //
        // שמירת מצב הבחירות בקובץ JSON לצורך התמדה
        // גם לאחר הפעלה מחדש של השרת.
        // =====================================================

        public ElectionStateService(IWebHostEnvironment environment)
        {
            var folder = Path.Combine(
                environment.ContentRootPath,
                "ElectionData");

            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "election-state.json");

            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);

                _state = JsonSerializer.Deserialize<ElectionState>(json)
                    ?? throw new InvalidOperationException(
                        "Election state file is invalid.");
            }
            else
            {
                _state = new ElectionState();
                Save();
            }

            if (_state.IsClosed && _state.Results == null)
            {
                throw new InvalidOperationException(
                    "Closed election has no saved results.");
            }
        }

        // =====================================================
        // 2. בדיקת מצב הבחירות
        //
        // קריאה מסונכרנת למצב הבחירות המשותף.
        // =====================================================

        public bool IsClosed
        {
            get
            {
                lock (_lock)
                {
                    return _state.IsClosed;
                }
            }
        }

        // =====================================================
        // 3. ביצוע הצבעה מסונכרנת
        //
        // שימוש ב-lock למניעת התנגשות בין הצבעה
        // לבין סגירת הבחירות בזמן בקשות מקבילות.
        // =====================================================

        public void ExecuteVote(Action castVote)
        {
            lock (_lock)
            {
                if (_state.IsClosed)
                {
                    throw new InvalidOperationException(
                        "ההצבעה הסתיימה ולא ניתן להצביע.");
                }

                castVote();
            }
        }

        // =====================================================
        // 4. קבלת תוצאות הבחירות
        //
        // התוצאות זמינות רק לאחר סגירת הבחירות.
        // מוחזר עותק של הנתונים למניעת שינוי המצב הפנימי.
        // =====================================================

        public ElectionResultsDto GetResults()
        {
            lock (_lock)
            {
                if (!_state.IsClosed || _state.Results == null)
                {
                    throw new InvalidOperationException(
                        "התוצאות יתפרסמו רק לאחר סגירת ההצבעה.");
                }

                return CloneResults(_state.Results);
            }
        }

        // =====================================================
        // 5. סגירת הבחירות וחישוב התוצאות
        //
        // חישוב התוצאות מתבצע תחת נעילה משותפת
        // ומצב הבחירות נשמר באופן קבוע בקובץ.
        // =====================================================

        public ElectionResultsDto CloseElection(
            Func<ElectionResultsDto> calculateResults)
        {
            lock (_lock)
            {
                if (_state.IsClosed)
                {
                    return CloneResults(_state.Results!);
                }

                var results = calculateResults();

                if (results == null)
                {
                    throw new InvalidOperationException(
                        "לא ניתן לחשב את תוצאות הבחירות.");
                }

                var previousState = _state;

                _state = new ElectionState
                {
                    IsClosed = true,
                    ClosedAt = DateTime.UtcNow,
                    Results = CloneResults(results)
                };

                try
                {
                    Save();
                }
                catch
                {
                    _state = previousState;
                    throw;
                }

                return CloneResults(_state.Results!);
            }
        }

        // =====================================================
        // 6. פתיחת הבחירות מחדש
        //
        // שינוי מצב הבחירות ומחיקת התוצאות המחושבות.
        // ההצבעות המקוריות במסד הנתונים נשמרות.
        // =====================================================

        public void OpenElection()
        {
            lock (_lock)
            {
                if (!_state.IsClosed)
                {
                    return;
                }

                var previousState = _state;

                _state = new ElectionState
                {
                    IsClosed = false,
                    ClosedAt = null,
                    Results = null
                };

                try
                {
                    Save();
                }
                catch
                {
                    _state = previousState;
                    throw;
                }
            }
        }

        // =====================================================
        // 7. יצירת עותק של תוצאות הבחירות
        //
        // העתקה עמוקה באמצעות Serialization
        // לצורך הפרדה בין הנתונים המוחזרים למצב הפנימי.
        // =====================================================

        private ElectionResultsDto CloneResults(
            ElectionResultsDto results)
        {
            var json = JsonSerializer.Serialize(results);

            return JsonSerializer.Deserialize<ElectionResultsDto>(json)!;
        }

        // =====================================================
        // 8. שמירת מצב הבחירות
        //
        // כתיבה לקובץ זמני והחלפת קובץ המצב הקיים.
        // מצמצמת סיכון לפגיעה בקובץ בעת שמירה.
        // =====================================================

        private void Save()
        {
            var json = JsonSerializer.Serialize(
                _state,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            var tempPath = _filePath + ".tmp";

            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _filePath, true);
        }

        // =====================================================
        // 9. מודל מצב הבחירות
        //
        // כולל סטטוס פתיחה/סגירה, מועד סגירה
        // ותוצאות הבחירות המחושבות.
        // =====================================================

        private class ElectionState
        {
            public bool IsClosed { get; set; } = false;
            public DateTime? ClosedAt { get; set; }
            public ElectionResultsDto? Results { get; set; }
        }
    }
}
