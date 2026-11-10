namespace DTO.Voting
{
    public class CandidateResultDto
    {
        public int CandidateId { get; set; }
        public string Name { get; set; } = string.Empty;

        // המיקום ברשימה הסופית
        public int Position { get; set; }

        // מספר הפעמים שהמועמד הופיע בפתקים
        public int TotalVotes { get; set; }
    }
}