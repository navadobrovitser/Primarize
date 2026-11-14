
using System;

namespace DTO.Posts
{
    public class PostDto
    {
        public int PostId { get; set; }

        public int CandidateId { get; set; }

        // שם המועמד
        public string CandidateName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string? MediaUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public int LikesCount { get; set; }

        public int DislikesCount { get; set; }

        public int CommentsCount { get; set; }
    }
}
