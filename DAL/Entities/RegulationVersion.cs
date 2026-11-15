using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
    public class RegulationVersion
    {
        [Key]
        public int VersionId { get; set; }

        [Required]
        [MaxLength(20)]
        public string VersionNumber { get; set; } = "1.0";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public DateTime? PublishedAt { get; set; }

        public List<RegulationChunk> Chunks { get; set; }
            = new List<RegulationChunk>();
    }
}