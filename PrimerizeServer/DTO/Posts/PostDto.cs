using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Posts
{
    public class PostDto
    {
        public int PostId { get; set; }
        public int CandidateId { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int CommentsCount { get; set; }
    }
}
