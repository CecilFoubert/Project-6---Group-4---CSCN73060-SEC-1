using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    public class CpuCooler
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
        public string? FanRPM { get; set; }
        
        [MaxLength(100)]
        public string? NoiseLevel { get; set; }
        
        [MaxLength(100)]
        public string? Color { get; set; }
        
        [MaxLength(100)]
        public string? Height { get; set; }
        
        [MaxLength(500)]
        public string? CPUSocket { get; set; }
        
        [MaxLength(200)]
        public string? WaterCooled { get; set; }
        
        [MaxLength(100)]
        public string? Fanless { get; set; }
        
        [MaxLength(100)]
        public string? SpecsNumber { get; set; }
    }
}

