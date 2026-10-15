using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Comment 
{
    public class CreateCommentDto
    {
        public int PostId { get; set; }        // מזהה הפוסט אליו משתייכת התגובה
        public string Content { get; set; } = string.Empty; // תוכן התגובה
    }
}