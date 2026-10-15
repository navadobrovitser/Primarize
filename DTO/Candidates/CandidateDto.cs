namespace DTO.Candidates
{
    public class CandidateDto
    {
        public int CandidateId { get; set; } // המפתח הפנימי (אפשר להשאיר ברקע או להציג)
        public int UserId { get; set; }     // המפתח הזר המספרי
        public string Id { get; set; }        // תעודת הזהות האמיתית של המשתמש
        public string Name { get; set; }
        public string Slogan { get; set; }
        public string Resume { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? ExplanationVideoUrl { get; set; }
    }
}