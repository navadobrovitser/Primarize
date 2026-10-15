namespace DTO.Likes
{
    public class PostLikeDto
    {
        public int PostLikeId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}