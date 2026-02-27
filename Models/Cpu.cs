/*!
 * @file Models/Cpu.cs
 * @brief CPU domain model.
 * @ingroup Models
 */

using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    /// <summary>
    /// CPU product model with common specification fields used by the application and API.
    /// Price is stored as a formatted string (e.g. "$199.99") to preserve display formatting.
    /// </summary>
    public class Cpu
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Display name of the CPU.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional product image URL.
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// Optional manufacturer/product page URL.
        /// </summary>
        [MaxLength(500)]
        public string? ProductUrl { get; set; }
        
        /// <summary>
        /// Price string (kept as string to preserve currency symbol and formatting).
        /// </summary>
        [MaxLength(100)]
        public string? Price { get; set; }
        
        /// <summary>
        /// Manufacturer name (e.g. "Intel", "AMD").
        /// </summary>
        [MaxLength(200)]
        public string? Manufacturer { get; set; }
        
        /// <summary>
        /// Manufacturer part number or SKU.
        /// </summary>
        [MaxLength(200)]
        public string? PartNumber { get; set; }
        
        /// <summary>
        /// Series or family name.
        /// </summary>
        [MaxLength(200)]
        public string? Series { get; set; }
        
        [MaxLength(200)]
        public string? Microarchitecture { get; set; }
        
        [MaxLength(200)]
        public string? CoreFamily { get; set; }
        
        /// <summary>
        /// CPU socket identifier (e.g. "LGA1700", "AM5").
        /// </summary>
        [MaxLength(100)]
        public string? Socket { get; set; }
        
        /// <summary>
        /// Optional core count.
        /// </summary>
        public int? CoreCount { get; set; }
        
        [MaxLength(100)]
        public string? PerformanceCoreClock { get; set; }
        
        [MaxLength(100)]
        public string? PerformanceCoreBoostClock { get; set; }
        
        [MaxLength(100)]
        public string? EfficiencyCoreClock { get; set; }
        
        [MaxLength(100)]
        public string? EfficiencyCoreBoostClock { get; set; }
        
        [MaxLength(100)]
        public string? L2Cache { get; set; }
        
        [MaxLength(100)]
        public string? L3Cache { get; set; }
        
        [MaxLength(100)]
        public string? TDP { get; set; }
        
        [MaxLength(200)]
        public string? IntegratedGraphics { get; set; }
        
        [MaxLength(100)]
        public string? MaximumSupportedMemory { get; set; }
        
        [MaxLength(100)]
        public string? ECCSupport { get; set; }
        
        [MaxLength(100)]
        public string? IncludesCooler { get; set; }
        
        [MaxLength(100)]
        public string? Packaging { get; set; }
        
        [MaxLength(100)]
        public string? Lithography { get; set; }
        
        [MaxLength(100)]
        public string? IncludesCPUCooler { get; set; }
        
        [MaxLength(200)]
        public string? SimultaneousMultithreading { get; set; }
    }
}

