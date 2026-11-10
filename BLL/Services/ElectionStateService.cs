
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

        public ElectionResultsDto CloseElection(
            Func<ElectionResultsDto> calculateResults)
        {
            lock (_lock)
            {
                if (_state.IsClosed)
                {
                    return CloneResults(_state.Results!);
                }

                // חישוב חד-פעמי של התוצאות
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

        private ElectionResultsDto CloneResults(
            ElectionResultsDto results)
        {
            var json = JsonSerializer.Serialize(results);

            return JsonSerializer.Deserialize<ElectionResultsDto>(json)!;
        }

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

        private class ElectionState
        {
            public bool IsClosed { get; set; } = false;
            public DateTime? ClosedAt { get; set; }
            public ElectionResultsDto? Results { get; set; }
        }
    }
}
