using System.ComponentModel.DataAnnotations;


namespace DTO
{
    public class ChatRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one message is required")]
        public List<ChatMessageDto> Messages { get; set; } = new();
    }
}