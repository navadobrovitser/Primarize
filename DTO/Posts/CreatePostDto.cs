using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Http;

namespace DTO.Posts
{
    public class CreatePostDto
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
        public int CandidateId { get; set; } // הוספת השדה החסר
        public IFormFile? MediaFile { get; set; }
    }
}

