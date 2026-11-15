
using System.ComponentModel.DataAnnotations;

namespace DTO.Comment
{
    public class CreateCommentDto
    {
        [Range(1, int.MaxValue)]
        public int PostId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; }
    }
}
