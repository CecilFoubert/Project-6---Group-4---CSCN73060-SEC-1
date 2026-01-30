using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    public class Case
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
        public string? Type { get; set; }
        
        [MaxLength(100)]
        public string? Color { get; set; }
        
        [MaxLength(100)]
        public string? PowerSupply { get; set; }
        
        [MaxLength(200)]
        public string? SidePanel { get; set; }
        
        [MaxLength(100)]
        public string? PowerSupplyShroud { get; set; }
        
        [MaxLength(200)]
        public string? FrontPanelUSB { get; set; }
        
        [MaxLength(200)]
        public string? MotherboardFormFactor { get; set; }
        
        [MaxLength(200)]
        public string? MaximumVideoCardLength { get; set; }
        
        [MaxLength(200)]
        public string? DriveBays { get; set; }
        
        [MaxLength(200)]
        public string? ExpansionSlots { get; set; }
        
        [MaxLength(300)]
        public string? Dimensions { get; set; }
        
        [MaxLength(100)]
        public string? Volume { get; set; }
    }
}

