using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    public class Motherboard
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        
        [MaxLength(500)]
        public string? ProductUrl { get; set; }
        
        [MaxLength(100)]
        public string? Price { get; set; }
        
        [MaxLength(500)]
        public string? Manufacturer { get; set; }
        
        [MaxLength(500)]
        public string? PartNumber { get; set; }
        
        [MaxLength(200)]
        public string? SocketCPU { get; set; }
        
        [MaxLength(200)]
        public string? FormFactor { get; set; }
        
        [MaxLength(200)]
        public string? Chipset { get; set; }
        
        [MaxLength(100)]
        public string? MemoryMax { get; set; }
        
        [MaxLength(100)]
        public string? MemoryType { get; set; }
        
        [MaxLength(100)]
        public string? MemorySlots { get; set; }
        
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

