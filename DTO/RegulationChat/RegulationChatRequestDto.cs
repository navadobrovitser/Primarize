using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.RegulationChat
{
    // =====================================================
    // בקשה לצ'אט התקנון
    //
    // Question היא השאלה הנוכחית.
    // PreviousMessages מאפשרות שאלות המשך באותה שיחה.
    // ההיסטוריה אינה נשמרת במסד הנתונים.
    // =====================================================

    public class RegulationChatRequestDto
    {
        [Required]
        [MaxLength(2000)]
        public string Question { get; set; } = string.Empty;

        public List<RegulationChatMessageDto> PreviousMessages
        { get; set; } = new();
    }
}
