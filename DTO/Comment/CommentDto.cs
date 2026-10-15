using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace DTO.Comment
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty; // שם המשתמש שכתב את התגובה (נוח להצגה במסך)
        public string Content { get; set; } = string.Empty; // תוכן התגובה
        public DateTime CreatedAt { get; set; } // תאריך ושעת יצירת התגובה
    }
}
