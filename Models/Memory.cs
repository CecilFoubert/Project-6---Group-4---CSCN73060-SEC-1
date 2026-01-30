using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    public class Memory
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

