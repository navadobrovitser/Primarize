using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.RegulationChat
{

    // =====================================================
    // תשובת הצ'אט
    // =====================================================

    public class RegulationChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;

        public int VersionId { get; set; }

        public string VersionNumber { get; set; } = string.Empty;

        public DateTime? PublishedAt { get; set; }

        public List<RegulationChatSourceDto> Sources
        { get; set; } = new();
    }
}
