using Microsoft.EntityFrameworkCore;
using Project_6___Group_4___CSCN73060_SEC_1.Models;

namespace Project_6___Group_4___CSCN73060_SEC_1.Data;

/// <summary>
/// Seeds minimal test data via EF (for InMemory database in unit tests).
/// </summary>
public class TestDbSeeder : DbSeeder
{
    private readonly ApplicationDbContext _context;

    public TestDbSeeder(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        ILogger<TestDbSeeder> logger,
        IConfiguration configuration)
        : base(context, env, logger, configuration)
    {
        _context = context;
    }

    public override async Task SeedAsync()
    {
        if (_context.Cpus.Any())
            return;

        _context.Cpus.AddRange(
            new Cpu
            {
                Name = "Intel Core i5-12400",
                Manufacturer = "Intel",
                Price = "199.99",
                Socket = "LGA1700",
                CoreCount = 6
            },
            new Cpu
            {
                Name = "AMD Ryzen 5 5600X",
                Manufacturer = "AMD",
                Price = "249.99",
                Socket = "AM4",
                CoreCount = 6
            },
            new Cpu
            {
                Name = "Intel Core i7-13700K",
                Manufacturer = "Intel",
                Price = "409.99",
                Socket = "LGA1700",
                CoreCount = 16
            });

        await _context.SaveChangesAsync();
    }
}
