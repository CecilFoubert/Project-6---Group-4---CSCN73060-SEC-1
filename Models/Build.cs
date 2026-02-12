using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    public class Build
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys (nullable - not all parts are required)
        public int? CpuId { get; set; }
        public int? GpuId { get; set; }
        public int? MotherboardId { get; set; }
        public int? MemoryId { get; set; }
        public int? StorageId { get; set; }
        public int? CaseId { get; set; }
        public int? PowerSupplyId { get; set; }
        public int? CpuCoolerId { get; set; }

        // Navigation Properties
        [ForeignKey("CpuId")]
        public virtual Cpu? Cpu { get; set; }

        [ForeignKey("GpuId")]
        public virtual Gpu? Gpu { get; set; }

        [ForeignKey("MotherboardId")]
        public virtual Motherboard? Motherboard { get; set; }

        [ForeignKey("MemoryId")]
        public virtual Memory? Memory { get; set; }

        [ForeignKey("StorageId")]
        public virtual Storage? Storage { get; set; }

        [ForeignKey("CaseId")]
        public virtual Case? Case { get; set; }

        [ForeignKey("PowerSupplyId")]
        public virtual PowerSupply? PowerSupply { get; set; }

        [ForeignKey("CpuCoolerId")]
        public virtual CpuCooler? CpuCooler { get; set; }

        // Computed property for total price
        [NotMapped]
        public decimal? TotalPrice
        {
            get
            {
                decimal total = 0;
                bool hasAnyPrice = false;

                if (Cpu?.Price != null && decimal.TryParse(Cpu.Price.Replace("$", "").Replace(",", ""), out var cpuPrice))
                {
                    total += cpuPrice;
                    hasAnyPrice = true;
                }

                if (Gpu?.Price != null && decimal.TryParse(Gpu.Price.Replace("$", "").Replace(",", ""), out var gpuPrice))
                {
                    total += gpuPrice;
                    hasAnyPrice = true;
                }

                if (Motherboard?.Price != null && decimal.TryParse(Motherboard.Price.Replace("$", "").Replace(",", ""), out var mbPrice))
                {
                    total += mbPrice;
                    hasAnyPrice = true;
                }

                if (Memory?.Price != null && decimal.TryParse(Memory.Price.Replace("$", "").Replace(",", ""), out var memPrice))
                {
                    total += memPrice;
                    hasAnyPrice = true;
                }

                if (Storage?.Price != null && decimal.TryParse(Storage.Price.Replace("$", "").Replace(",", ""), out var storagePrice))
                {
                    total += storagePrice;
                    hasAnyPrice = true;
                }

                if (Case?.Price != null && decimal.TryParse(Case.Price.Replace("$", "").Replace(",", ""), out var casePrice))
                {
                    total += casePrice;
                    hasAnyPrice = true;
                }

                if (PowerSupply?.Price != null && decimal.TryParse(PowerSupply.Price.Replace("$", "").Replace(",", ""), out var psuPrice))
                {
                    total += psuPrice;
                    hasAnyPrice = true;
                }

                if (CpuCooler?.Price != null && decimal.TryParse(CpuCooler.Price.Replace("$", "").Replace(",", ""), out var coolerPrice))
                {
                    total += coolerPrice;
                    hasAnyPrice = true;
                }

                return hasAnyPrice ? total : null;
            }
        }
    }
}
