using System.ComponentModel.DataAnnotations;

namespace DTO.RegulationManagement
{
    // הנתונים שהמנהל שולח כשהוא שומר סעיף.
    public class UpdateRegulationSectionRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}