using Microsoft.EntityFrameworkCore;
using Project_6___Group_4___CSCN73060_SEC_1.Models;

namespace Project_6___Group_4___CSCN73060_SEC_1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // PC Component DbSets
        public DbSet<Cpu> Cpus { get; set; }
        public DbSet<Gpu> Gpus { get; set; }
        public DbSet<Memory> Memories { get; set; }
        public DbSet<Motherboard> Motherboards { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Storage> Storages { get; set; }
        public DbSet<PowerSupply> PowerSupplies { get; set; }
        public DbSet<CpuCooler> CpuCoolers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure your entities here if needed
            modelBuilder.Entity<Cpu>().ToTable("Cpus");
            modelBuilder.Entity<Gpu>().ToTable("Gpus");
            modelBuilder.Entity<Memory>().ToTable("Memories");
            modelBuilder.Entity<Motherboard>().ToTable("Motherboards");
            modelBuilder.Entity<Case>().ToTable("Cases");
            modelBuilder.Entity<Storage>().ToTable("Storages");
            modelBuilder.Entity<PowerSupply>().ToTable("PowerSupplies");
            modelBuilder.Entity<CpuCooler>().ToTable("CpuCoolers");
        }
    }
}

