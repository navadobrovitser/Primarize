using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.RegulationChat
{
    // =====================================================
    // מקור מתוך התקנון שהוחזר בחיפוש.
    //
    // מאפשר להציג למשתמש את הסעיף המלא
    // ולא רק את מספרו.
    // =====================================================

    public class RegulationChatSourceDto
    {
        public string SectionNumber { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}
