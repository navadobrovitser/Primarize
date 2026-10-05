using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Entities
{
    public class PostComment
    {
        // מזהה ייחודי של רשומת הקישור

        
            // מפתחות זרים
            public int CommentId { get; set; }
            public int PostId { get; set; }

            // Navigation Properties
            public Comment Comment { get; set; } = null!;
            public Post Post { get; set; } = null!;
        }
    }

