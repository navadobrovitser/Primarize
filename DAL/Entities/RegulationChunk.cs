using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    public class RegulationChunk
    {
        [Key]
        public int ChunkId { get; set; }

        public int VersionId { get; set; }

        [ForeignKey(nameof(VersionId))]
        public RegulationVersion Version { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string SectionNumber { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        // הווקטור יישמר כמחרוזת JSON
        public string? Embedding { get; set; }
    }
}