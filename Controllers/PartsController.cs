using Microsoft.AspNetCore.Mvc;
using Project_6___Group_4___CSCN73060_SEC_1.Models;
using Project_6___Group_4___CSCN73060_SEC_1.Services;

namespace Project_6___Group_4___CSCN73060_SEC_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;
        private readonly ILogger<PartsController> _logger;

        public PartsController(IPartService partService, ILogger<PartsController> logger)
        {
            _partService = partService;
            _logger = logger;
        }

        /// <summary>
        /// Get all parts of a specific type
        /// GET /api/parts/{partType}
        /// Examples: /api/parts/cpu, /api/parts/gpu, /api/parts/memory
        /// </summary>
        [HttpGet("{partType}")]
        public async Task<IActionResult> GetAll(string partType)
        {
            try
            {
                var parts = await _partService.GetAllAsync(partType);
                return Ok(new
                {
                    partType,
                    count = parts.Count(),
                    data = parts
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts of type {PartType}", partType);
                return StatusCode(500, new { error = "An error occurred while fetching parts" });
            }
        }

        /// <summary>
        /// Get a specific part by ID
        /// GET /api/parts/{partType}/{id}
        /// Example: /api/parts/cpu/5
        /// </summary>
        [HttpGet("{partType}/{id:int}")]
        public async Task<IActionResult> GetById(string partType, int id)
        {
            try
            {
                var part = await _partService.GetByIdAsync(partType, id);
                if (part == null)
                    return NotFound(new { error = $"Part with ID {id} not found in {partType}" });

                return Ok(part);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part {PartType}/{Id}", partType, id);
                return StatusCode(500, new { error = "An error occurred while fetching the part" });
            }
        }

        /// <summary>
        /// Create a new part
        /// POST /api/parts/{partType}
        /// Body: { "name": "...", "price": "...", "manufacturer": "..." }
        /// </summary>
        [HttpPost("{partType}")]
        public async Task<IActionResult> Create(string partType, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                var createdPart = await _partService.CreateAsync(partType, data);
                return CreatedAtAction(
                    nameof(GetById),
                    new { partType, id = ((dynamic)createdPart).Id },
                    createdPart
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part of type {PartType}", partType);
                return StatusCode(500, new { error = "An error occurred while creating the part" });
            }
        }

        /// <summary>
        /// Update an existing part
        /// PUT /api/parts/{partType}/{id}
        /// Body: { "name": "...", "price": "..." }
        /// </summary>
        [HttpPut("{partType}/{id:int}")]
        public async Task<IActionResult> Update(string partType, int id, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                var updatedPart = await _partService.UpdateAsync(partType, id, data);
                if (updatedPart == null)
                    return NotFound(new { error = $"Part with ID {id} not found in {partType}" });

                return Ok(updatedPart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part {PartType}/{Id}", partType, id);
                return StatusCode(500, new { error = "An error occurred while updating the part" });
            }
        }

        /// <summary>
        /// Delete a part
        /// DELETE /api/parts/{partType}/{id}
        /// </summary>
        [HttpDelete("{partType}/{id:int}")]
        public async Task<IActionResult> Delete(string partType, int id)
        {
            try
            {
                var deleted = await _partService.DeleteAsync(partType, id);
                if (!deleted)
                    return NotFound(new { error = $"Part with ID {id} not found in {partType}" });

                return Ok(new { message = $"Part {id} deleted successfully from {partType}" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part {PartType}/{Id}", partType, id);
                return StatusCode(500, new { error = "An error occurred while deleting the part" });
            }
        }

        /// <summary>
        /// Get supported part types
        /// GET /api/parts
        /// </summary>
        [HttpGet]
        public IActionResult GetSupportedTypes()
        {
            return Ok(new
            {
                message = "Supported part types",
                types = new[]
                {
                    "cpu", "gpu", "memory", "motherboard",
                    "case", "storage", "powersupply", "cpucooler"
                }
            });
        }

        /// <summary>
        /// Get all available filter options for a part type
        /// GET /api/parts/{partType}/filters
        /// Returns all searchable attributes with their distinct values
        /// Perfect for building dynamic dropdown filters in the frontend
        /// 
        /// Examples:
        /// - /api/parts/cpu/filters
        /// - /api/parts/gpu/filters
        /// - /api/parts/memory/filters
        /// </summary>
        [HttpGet("{partType}/filters")]
        public async Task<IActionResult> GetFilters(string partType)
        {
            try
            {
                var filterOptions = await _partService.GetFilterOptionsAsync(partType);
                
                return Ok(new
                {
                    partType = filterOptions.PartType,
                    totalAttributes = filterOptions.Attributes.Count,
                    attributes = filterOptions.Attributes
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filter options for part type {PartType}", partType);
                return StatusCode(500, new { error = "An error occurred while fetching filter options" });
            }
        }

        /// <summary>
        /// Dynamic search endpoint for parts with flexible filtering (JSON body)
        /// POST /api/parts/{partType}/search
        /// Body: SearchFilters object with minPrice, maxPrice, manufacturer, and dynamic filters
        /// 
        /// Example body:
        /// {
        ///   "minPrice": 200,
        ///   "maxPrice": 500,
        ///   "manufacturer": "Intel",
        ///   "filters": {
        ///     "socket": "LGA1700",
        ///     "cores": "8"
        ///   }
        /// }
        /// </summary>
        [HttpPost("{partType}/search")]
        public async Task<IActionResult> Search(string partType, [FromBody] SearchFilters? searchFilters)
        {
            try
            {
                // Convert SearchFilters to Dictionary for the service
                var filters = new Dictionary<string, string>();
                
                if (searchFilters != null)
                {
                    if (searchFilters.MinPrice.HasValue)
                        filters["minPrice"] = searchFilters.MinPrice.Value.ToString();
                    
                    if (searchFilters.MaxPrice.HasValue)
                        filters["maxPrice"] = searchFilters.MaxPrice.Value.ToString();
                    
                    if (!string.IsNullOrWhiteSpace(searchFilters.Manufacturer))
                        filters["manufacturer"] = searchFilters.Manufacturer;
                    
                    if (searchFilters.Filters != null)
                    {
                        foreach (var kvp in searchFilters.Filters)
                        {
                            if (!string.IsNullOrWhiteSpace(kvp.Value))
                                filters[kvp.Key] = kvp.Value;
                        }
                    }
                }

                var searchResult = await _partService.SearchAsync(partType, filters);
                
                return Ok(new
                {
                    partType = searchResult.PartType,
                    totalCount = searchResult.TotalCount,
                    appliedFilters = searchResult.AppliedFilters,
                    results = searchResult.Results,
                    averagePart = searchResult.AveragePart
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching parts of type {PartType}", partType);
                return StatusCode(500, new { error = "An error occurred while searching parts" });
            }
        }
    }
}

