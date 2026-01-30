using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    public class PowerSupply
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
        
        [MaxLength(200)]
        public string? Model { get; set; }
        
        [MaxLength(500)]
        public string? PartNumber { get; set; }
        
        [MaxLength(100)]
        public string? Type { get; set; }
        
        [MaxLength(100)]
        public string? EfficiencyRating { get; set; }
        
        [MaxLength(100)]
        public string? Wattage { get; set; }
        
        [MaxLength(100)]
        public string? Length { get; set; }
        
        [MaxLength(100)]
        public string? Modular { get; set; }
        
        [MaxLength(100)]
        public string? Color { get; set; }
        
        [MaxLength(100)]
        public string? Fanless { get; set; }
        
        [MaxLength(100)]
        public string? ATX4PinConnectors { get; set; }
        
        [MaxLength(100)]
        public string? EPS8PinConnectors { get; set; }
        
        [MaxLength(100)]
        public string? PCIe12Plus4Pin12VHPWRConnectors { get; set; }
        
        [MaxLength(100)]
        public string? PCIe12PinConnectors { get; set; }
        
        [MaxLength(100)]
        public string? PCIe8PinConnectors { get; set; }
        
        [MaxLength(100)]
        public string? PCIe6Plus2PinConnectors { get; set; }
        
        [MaxLength(100)]
        public string? PCIe6PinConnectors { get; set; }
        
        [MaxLength(100)]
        public string? SATAConnectors { get; set; }
        
        [MaxLength(100)]
        public string? Molex4PinConnectors { get; set; }
        
        [MaxLength(100)]
        public string? SpecsNumber { get; set; }
    }
}

