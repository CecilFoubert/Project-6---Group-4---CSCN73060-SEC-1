/*!
 * @file Models/PartAverageCache.cs
 * @brief Caching model for average-part results (search).
 * @ingroup Models
 */

using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    /// <summary>
    /// Caches calculated average parts for search results.
    /// Lookup key: PartType + FilterKey (deterministic hash of applied filters).
    /// </summary>
    public class PartAverageCache
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string PartType { get; set; } = string.Empty;

        /// <summary>
        /// Deterministic key from sorted filter key-value pairs (e.g. hash or "key1=val1;key2=val2").
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string FilterKey { get; set; } = string.Empty;

        /// <summary>
        /// JSON-serialized average part data (Dictionary/ExpandoObject).
        /// </summary>
        [Required]
        public string AveragePartJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
