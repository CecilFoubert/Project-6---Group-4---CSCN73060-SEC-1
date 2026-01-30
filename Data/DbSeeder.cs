using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Project_6___Group_4___CSCN73060_SEC_1.Data
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DbSeeder> _logger;
        private readonly IConfiguration _configuration;

        public DbSeeder(
            ApplicationDbContext context, 
            IWebHostEnvironment env, 
            ILogger<DbSeeder> logger,
            IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if data already exists
                if (_context.Cpus.Any())
                {
                    _logger.LogInformation("Database already seeded. Skipping seed process.");
                    return;
                }

                _logger.LogInformation("Starting database seeding from SQL file...");

                var sqlFilePath = Path.Combine(_env.ContentRootPath, "Data", "SeedData", "pcpartpicker_seed_data.sql");

                if (!File.Exists(sqlFilePath))
                {
                    _logger.LogError($"SQL seed file not found: {sqlFilePath}");
                    return;
                }

                // Read the SQL file
                var sqlContent = await File.ReadAllTextAsync(sqlFilePath);
                
                // Split into individual statements (separated by semicolons)
                var statements = sqlContent
                    .Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s) && !s.TrimStart().StartsWith("--"))
                    .ToList();

                _logger.LogInformation($"Executing {statements.Count} SQL statements...");

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    var batchSize = 100;
                    var totalBatches = (int)Math.Ceiling(statements.Count / (double)batchSize);
                    
                    for (int batch = 0; batch < totalBatches; batch++)
                    {
                        var batchStatements = statements
                            .Skip(batch * batchSize)
                            .Take(batchSize)
                            .ToList();

                        using (var transaction = await connection.BeginTransactionAsync())
                        {
                            try
                            {
                                foreach (var statement in batchStatements)
                                {
                                    var trimmedStatement = statement.Trim();
                                    if (string.IsNullOrEmpty(trimmedStatement))
                                        continue;

                                    using (var command = new MySqlCommand(trimmedStatement, connection, transaction))
                                    {
                                        command.CommandTimeout = 300; // 5 minutes timeout
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }

                                await transaction.CommitAsync();
                                _logger.LogInformation($"Batch {batch + 1}/{totalBatches} completed.");
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError(ex, $"Error executing batch {batch + 1}");
                                throw;
                            }
                        }
                    }
                }

                _logger.LogInformation("Database seeding completed successfully!");
                
                // Verify the data was inserted
                var cpuCount = await _context.Cpus.CountAsync();
                var gpuCount = await _context.Gpus.CountAsync();
                var memoryCount = await _context.Memories.CountAsync();
                
                _logger.LogInformation($"Seeded records - CPUs: {cpuCount}, GPUs: {gpuCount}, Memory: {memoryCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
