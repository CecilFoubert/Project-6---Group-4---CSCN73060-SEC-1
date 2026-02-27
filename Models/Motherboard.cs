/*!
 * @file Models/Motherboard.cs
 * @brief Motherboard domain model.
 * @ingroup Models
 */

using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    /// <summary>
    /// Represents a motherboard product and its technical specifications.
    /// Used by the UI and API to display motherboard details and for compatibility checks.
    /// </summary>
    public class Motherboard
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Human-friendly name of the motherboard (e.g. "ASUS ROG Strix Z790-E").
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// URL to a product image.
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// Link to the manufacturer/product page.
        /// </summary>
        [MaxLength(500)]
        public string? ProductUrl { get; set; }
        
        /// <summary>
        /// Price as shown on the site (kept as string to preserve formatting).
        /// </summary>
        [MaxLength(100)]
        public string? Price { get; set; }
        
        /// <summary>
        /// Manufacturer name.
        /// </summary>
        [MaxLength(500)]
        public string? Manufacturer { get; set; }
        
        /// <summary>
        /// Manufacturer part number or SKU.
        /// </summary>
        [MaxLength(500)]
        public string? PartNumber { get; set; }
        
        /// <summary>
        /// CPU socket(s) supported (e.g. "LGA1700").
        /// </summary>
        [MaxLength(200)]
        public string? SocketCPU { get; set; }
        
        /// <summary>
        /// Board form factor (e.g. "ATX", "mATX").
        /// </summary>
        [MaxLength(200)]
        public string? FormFactor { get; set; }
        
        /// <summary>
        /// Chipset identifier (e.g. "Z790").
        /// </summary>
        [MaxLength(200)]
        public string? Chipset { get; set; }
        
        /// <summary>
        /// Maximum supported memory (e.g. "128 GB").
        /// </summary>
        [MaxLength(100)]
        public string? MemoryMax { get; set; }
        
        /// <summary>
        /// Memory types supported (e.g. "DDR5").
        /// </summary>
        [MaxLength(100)]
        public string? MemoryType { get; set; }
        
        /// <summary>
        /// Number of memory slots (e.g. "4") or description.
        /// </summary>
        [MaxLength(100)]
        public string? MemorySlots { get; set; }
        
        /// <summary>
        /// Supported memory speeds or supported XMP profiles.
        /// Example: "DDR5-5200, DDR5-6000 (OC)".
        /// </summary>
        [MaxLength(500)]
        public string? MemorySpeed { get; set; }
        
        [MaxLength(100)]
        public string? Color { get; set; }
        
        [MaxLength(100)]
        public string? PCIex16Slots { get; set; }
        
        [MaxLength(100)]
        public string? PCIex8Slots { get; set; }
        
        [MaxLength(100)]
        public string? PCIex4Slots { get; set; }
        
        [MaxLength(100)]
        public string? PCIex1Slots { get; set; }
        
        [MaxLength(100)]
        public string? PCISlots { get; set; }
        
        [MaxLength(100)]
        public string? M2Slots { get; set; }
        
        [MaxLength(100)]
        public string? MiniPCIeSlots { get; set; }
        
        [MaxLength(100)]
        public string? HalfMiniPCIeSlots { get; set; }
        
        [MaxLength(100)]
        public string? MiniPCIemSATASlots { get; set; }
        
        [MaxLength(100)]
        public string? mSATASlots { get; set; }
        
        [MaxLength(100)]
        public string? SATA6GbsSlots { get; set; }
        
        [MaxLength(200)]
        public string? OnboardEthernet { get; set; }
        
        [MaxLength(200)]
        public string? OnboardVideo { get; set; }
        
        [MaxLength(100)]
        public string? USB20Headers { get; set; }
        
        [MaxLength(100)]
        public string? USB20HeadersSinglePort { get; set; }
        
        [MaxLength(100)]
        public string? USB32Gen1Headers { get; set; }
        
        [MaxLength(100)]
        public string? USB32Gen2Headers { get; set; }
        
        [MaxLength(100)]
        public string? USB32Gen2x2Headers { get; set; }
        
        [MaxLength(100)]
        public string? SupportsECC { get; set; }
        
        [MaxLength(200)]
        public string? WirelessNetworking { get; set; }
        
        [MaxLength(200)]
        public string? RAIDSupport { get; set; }
    }
}

