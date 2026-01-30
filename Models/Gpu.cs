using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    public class Gpu
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
        
        [MaxLength(200)]
        public string? Manufacturer { get; set; }
        
        [MaxLength(500)]
        public string? PartNumber { get; set; }
        
        [MaxLength(200)]
        public string? Chipset { get; set; }
        
        [MaxLength(100)]
        public string? Memory { get; set; }
        
        [MaxLength(100)]
        public string? MemoryType { get; set; }
        
        [MaxLength(100)]
        public string? CoreClock { get; set; }
        
        [MaxLength(100)]
        public string? BoostClock { get; set; }
        
        [MaxLength(100)]
        public string? EffectiveMemoryClock { get; set; }
        
        [MaxLength(100)]
        public string? Interface { get; set; }
        
        [MaxLength(100)]
        public string? Color { get; set; }
        
        [MaxLength(100)]
        public string? FrameSync { get; set; }
        
        [MaxLength(100)]
        public string? Length { get; set; }
        
        [MaxLength(100)]
        public string? TDP { get; set; }
        
        [MaxLength(100)]
        public string? CaseExpansionSlotWidth { get; set; }
        
        [MaxLength(100)]
        public string? TotalSlotWidth { get; set; }
        
        [MaxLength(200)]
        public string? Cooling { get; set; }
        
        [MaxLength(200)]
        public string? ExternalPower { get; set; }
        
        [MaxLength(100)]
        public string? HDMIOutputs { get; set; }
        
        [MaxLength(100)]
        public string? DisplayPortOutputs { get; set; }
        
        [MaxLength(100)]
        public string? DVIDDualLinkOutputs { get; set; }
        
        [MaxLength(100)]
        public string? HDMI21aOutputs { get; set; }
        
        [MaxLength(100)]
        public string? DisplayPort14Outputs { get; set; }
        
        [MaxLength(100)]
        public string? DisplayPort14aOutputs { get; set; }
        
        [MaxLength(100)]
        public string? DisplayPort21Outputs { get; set; }
        
        [MaxLength(100)]
        public string? SLICrossFire { get; set; }
    }
}

