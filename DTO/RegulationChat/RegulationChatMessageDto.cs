using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.RegulationChat
{
    // הודעה קודמת בשיחה.
    public class RegulationChatMessageDto
    {
        public string Role { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
    }
}
