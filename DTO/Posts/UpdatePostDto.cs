using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Http;

namespace DTO.Posts
{
    public class UpdatePostDto
    {

        public string Content { get; set; }
        public string ContentType { get; set; }
        public IFormFile? MediaFile { get; set; }
    }
}