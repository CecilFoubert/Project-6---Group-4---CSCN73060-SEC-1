using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_6___Group_4___CSCN73060_SEC_1.Data;
using Project_6___Group_4___CSCN73060_SEC_1.Models;

namespace Project_6___Group_4___CSCN73060_SEC_1.Controllers
{
    [ApiController]
    [Route("api/Builds")]
    public class ApiBuildsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiBuildsController> _logger;

        private static readonly Dictionary<string, string> PartTypeToColumn = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cpu"] = "CpuId",
            ["motherboard"] = "MotherboardId",
            ["memory"] = "MemoryId",
            ["storage"] = "StorageId",
            ["gpu"] = "GpuId",
            ["case"] = "CaseId",
            ["powersupply"] = "PowerSupplyId",
            ["cpucooler"] = "CpuCoolerId"
        };

        public ApiBuildsController(ApplicationDbContext context, ILogger<ApiBuildsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all builds
        /// GET /api/ApiBuilds
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var builds = await _context.Builds
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Memory)
                .Include(b => b.Storage)
                .Include(b => b.Case)
                .Include(b => b.PowerSupply)
                .Include(b => b.CpuCooler)
                .OrderByDescending(b => b.UpdatedAt)
                .ToListAsync();

            var result = builds.Select(b => ToApiModel(b));
            return Ok(result);
        }

        /// <summary>
        /// Get build by ID
        /// GET /api/ApiBuilds/5
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var build = await _context.Builds
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Memory)
                .Include(b => b.Storage)
                .Include(b => b.Case)
                .Include(b => b.PowerSupply)
                .Include(b => b.CpuCooler)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (build == null)
                return NotFound(new { error = $"Build with ID {id} not found" });

            return Ok(ToApiModel(build));
        }

        /// <summary>
        /// Get build by name (to check if a build with this name exists)
        /// GET /api/ApiBuilds/by-name/My%20Build
        /// </summary>
        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var build = await _context.Builds
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Memory)
                .Include(b => b.Storage)
                .Include(b => b.Case)
                .Include(b => b.PowerSupply)
                .Include(b => b.CpuCooler)
                .FirstOrDefaultAsync(b => b.Name == name);

            if (build == null)
                return NotFound(new { error = $"Build with name '{name}' not found" });

            return Ok(ToApiModel(build));
        }

        /// <summary>
        /// Create a new build
        /// POST /api/ApiBuilds
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BuildCreateUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { error = "Name is required" });

            var build = MapDtoToBuild(dto);
            build.CreatedAt = DateTime.UtcNow;
            build.UpdatedAt = DateTime.UtcNow;

            _context.Builds.Add(build);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = build.Id }, ToApiModel(build));
        }

        /// <summary>
        /// Update an existing build (e.g. when user saves a build with the same name as an existing one)
        /// PATCH /api/ApiBuilds/5
        /// </summary>
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] BuildCreateUpdateDto dto)
        {
            var build = await _context.Builds.FindAsync(id);
            if (build == null)
                return NotFound(new { error = $"Build with ID {id} not found" });

            if (!string.IsNullOrWhiteSpace(dto.Name))
                build.Name = dto.Name;

            if (dto.Description != null)
                build.Description = dto.Description;

            if (dto.Parts != null)
            {
                foreach (var kvp in dto.Parts)
                {
                    if (PartTypeToColumn.TryGetValue(kvp.Key, out var column))
                    {
                        var partId = kvp.Value?.Id;
                        switch (column)
                        {
                            case "CpuId": build.CpuId = partId; break;
                            case "GpuId": build.GpuId = partId; break;
                            case "MotherboardId": build.MotherboardId = partId; break;
                            case "MemoryId": build.MemoryId = partId; break;
                            case "StorageId": build.StorageId = partId; break;
                            case "CaseId": build.CaseId = partId; break;
                            case "PowerSupplyId": build.PowerSupplyId = partId; break;
                            case "CpuCoolerId": build.CpuCoolerId = partId; break;
                        }
                    }
                }
            }

            build.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updated = await _context.Builds
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Memory)
                .Include(b => b.Storage)
                .Include(b => b.Case)
                .Include(b => b.PowerSupply)
                .Include(b => b.CpuCooler)
                .FirstAsync(b => b.Id == id);

            return Ok(ToApiModel(updated));
        }

        private static Build MapDtoToBuild(BuildCreateUpdateDto dto)
        {
            var build = new Build
            {
                Name = dto.Name,
                Description = dto.Description
            };

            if (dto.Parts != null)
            {
                foreach (var kvp in dto.Parts)
                {
                    if (PartTypeToColumn.TryGetValue(kvp.Key, out var column))
                    {
                        var partId = kvp.Value?.Id;
                        switch (column)
                        {
                            case "CpuId": build.CpuId = partId; break;
                            case "GpuId": build.GpuId = partId; break;
                            case "MotherboardId": build.MotherboardId = partId; break;
                            case "MemoryId": build.MemoryId = partId; break;
                            case "StorageId": build.StorageId = partId; break;
                            case "CaseId": build.CaseId = partId; break;
                            case "PowerSupplyId": build.PowerSupplyId = partId; break;
                            case "CpuCoolerId": build.CpuCoolerId = partId; break;
                        }
                    }
                }
            }

            return build;
        }

        private static object ToApiModel(Build b)
        {
            var parts = new Dictionary<string, object?>();
            if (b.Cpu != null) parts["cpu"] = new { b.Cpu.Id, b.Cpu.Name, b.Cpu.Manufacturer, b.Cpu.Price, PartType = "cpu" };
            if (b.Gpu != null) parts["gpu"] = new { b.Gpu.Id, b.Gpu.Name, b.Gpu.Manufacturer, b.Gpu.Price, PartType = "gpu" };
            if (b.Motherboard != null) parts["motherboard"] = new { b.Motherboard.Id, b.Motherboard.Name, b.Motherboard.Manufacturer, b.Motherboard.Price, PartType = "motherboard" };
            if (b.Memory != null) parts["memory"] = new { b.Memory.Id, b.Memory.Name, b.Memory.Manufacturer, b.Memory.Price, PartType = "memory" };
            if (b.Storage != null) parts["storage"] = new { b.Storage.Id, b.Storage.Name, b.Storage.Manufacturer, b.Storage.Price, PartType = "storage" };
            if (b.Case != null) parts["case"] = new { b.Case.Id, b.Case.Name, b.Case.Manufacturer, b.Case.Price, PartType = "case" };
            if (b.PowerSupply != null) parts["powersupply"] = new { b.PowerSupply.Id, b.PowerSupply.Name, b.PowerSupply.Manufacturer, b.PowerSupply.Price, PartType = "powersupply" };
            if (b.CpuCooler != null) parts["cpucooler"] = new { b.CpuCooler.Id, b.CpuCooler.Name, b.CpuCooler.Manufacturer, b.CpuCooler.Price, PartType = "cpucooler" };

            return new
            {
                id = b.Id,
                name = b.Name,
                description = b.Description,
                parts,
                createdAt = b.CreatedAt,
                updatedAt = b.UpdatedAt
            };
        }
    }
}
