/*!
 * @file Models/Memory.cs
 * @brief Memory (RAM) domain model.
 * @ingroup Models
 */

using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    /// <summary>
    /// Memory (RAM) product model used by the API and UI.
    /// </summary>
    public class Memory
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Display name of the memory product.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        
        [MaxLength(500)]
        public string? ProductUrl { get; set; }
        
        /// <summary>
        /// Price displayed as formatted string (e.g. "$89.99").
        /// </summary>
        [MaxLength(100)]
        public string? Price { get; set; }
        
        [MaxLength(500)]
        public string? Manufacturer { get; set; }
        
        [MaxLength(500)]
        public string? PartNumber { get; set; }
        
        /// <summary>
        /// Speed description (e.g. "DDR5-5200").
        /// </summary>
        [MaxLength(200)]
        public string? Speed { get; set; }
        
        [MaxLength(200)]
        public string? FormFactor { get; set; }
        
        [MaxLength(200)]
        public string? Modules { get; set; }
        
        [MaxLength(100)]
        public string? PricePerGB { get; set; }
        
        [MaxLength(100)]
        public string? Color { get; set; }
        
        [MaxLength(100)]
        public string? FirstWordLatency { get; set; }
        
        [MaxLength(100)]
        public string? CASLatency { get; set; }
        
        [MaxLength(100)]
        public string? Voltage { get; set; }
        
        [MaxLength(200)]
        public string? Timing { get; set; }
        
        [MaxLength(200)]
        public string? ECCRegistered { get; set; }
        
        [MaxLength(100)]
        public string? HeatSpreader { get; set; }
        
        [MaxLength(100)]
        public string? SpecsNumber { get; set; }
    }
}

