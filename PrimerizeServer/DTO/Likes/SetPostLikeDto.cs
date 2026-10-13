using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Likes
{
    public class SetPostLikeDto
    {
        public int PostId { get; set; }
        public string Type
        { // או ה-Enum המתאים ללייק אם מוגדר ב-DAL
            get; set;
        }
    }
}