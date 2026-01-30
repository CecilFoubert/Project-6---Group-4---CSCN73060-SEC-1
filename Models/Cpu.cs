using System.ComponentModel.DataAnnotations;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    public class Cpu
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
        public string? PartNumber { get; set; }
        
        [MaxLength(200)]
        public string? Series { get; set; }
        
        [MaxLength(200)]
        public string? Microarchitecture { get; set; }
        
        [MaxLength(200)]
        public string? CoreFamily { get; set; }
        
        [MaxLength(100)]
        public string? Socket { get; set; }
        
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

