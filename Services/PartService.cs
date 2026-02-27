using Microsoft.EntityFrameworkCore;
using Project_6___Group_4___CSCN73060_SEC_1.Data;
using Project_6___Group_4___CSCN73060_SEC_1.Models;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Project_6___Group_4___CSCN73060_SEC_1.Services
{
    public class PartService : IPartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PartService> _logger;

        public PartService(ApplicationDbContext context, ILogger<PartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private Type? GetModelType(string partType)
        {
            return partType.ToLower() switch
            {
                "cpu" or "cpus" => typeof(Cpu),
                "gpu" or "gpus" => typeof(Gpu),
                "memory" or "memories" => typeof(Memory),
                "motherboard" or "motherboards" => typeof(Motherboard),
                "case" or "cases" => typeof(Case),
                "storage" or "storages" => typeof(Storage),
                "powersupply" or "powersupplies" => typeof(PowerSupply),
                "cpucooler" or "cpucoolers" => typeof(CpuCooler),
                _ => null
            };
        }

        private object ToLightweightObject(object entity, string partType)
        {
            var properties = entity.GetType().GetProperties();
            var result = new ExpandoObject() as IDictionary<string, object?>;

            // Add part type first
            result["PartType"] = partType;

            // Add all properties dynamically
            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(entity);
                    // Only include non-null values or keep null for important fields
                    if (value != null || property.Name == "Id" || property.Name == "Name" || property.Name == "Price")
                    {
                        result[property.Name] = value;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting property {PropertyName} from {EntityType}", 
                        property.Name, entity.GetType().Name);
                }
            }

            return result;
        }

        public async Task<IEnumerable<object>> GetAllAsync(string partType)
        {
            var normalizedPartType = partType.ToLower();
            return normalizedPartType switch
            {
                "cpu" or "cpus" => await GetAllAsyncInternal(_context.Cpus, "cpu"),
                "gpu" or "gpus" => await GetAllAsyncInternal(_context.Gpus, "gpu"),
                "memory" or "memories" => await GetAllAsyncInternal(_context.Memories, "memory"),
                "motherboard" or "motherboards" => await GetAllAsyncInternal(_context.Motherboards, "motherboard"),
                "case" or "cases" => await GetAllAsyncInternal(_context.Cases, "case"),
                "storage" or "storages" => await GetAllAsyncInternal(_context.Storages, "storage"),
                "powersupply" or "powersupplies" => await GetAllAsyncInternal(_context.PowerSupplies, "powersupply"),
                "cpucooler" or "cpucoolers" => await GetAllAsyncInternal(_context.CpuCoolers, "cpucooler"),
                _ => throw new ArgumentException($"Invalid part type: {partType}")
            };
        }

        private async Task<IEnumerable<object>> GetAllAsyncInternal<T>(DbSet<T> dbSet, string partType) where T : class
        {
            var items = await dbSet.ToListAsync();
            return items.Select(item => ToLightweightObject(item, partType));
        }

        public async Task<object?> GetByIdAsync(string partType, int id)
        {
            var modelType = GetModelType(partType);
            if (modelType == null)
                throw new ArgumentException($"Invalid part type: {partType}");

            var entity = await _context.FindAsync(modelType, id);
            return entity != null ? ToLightweightObject(entity, partType) : null;
        }

        public async Task<object> CreateAsync(string partType, Dictionary<string, object> data)
        {
            var modelType = GetModelType(partType);
            if (modelType == null)
                throw new ArgumentException($"Invalid part type: {partType}");

            var entity = Activator.CreateInstance(modelType);
            if (entity == null)
                throw new Exception("Failed to create entity instance");

            // Set properties from data dictionary
            foreach (var kvp in data)
            {
                var property = modelType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null && property.CanWrite)
                {
                    try
                    {
                        var value = Convert.ChangeType(kvp.Value, property.PropertyType);
                        property.SetValue(entity, value);
                    }
                    catch
                    {
                        property.SetValue(entity, kvp.Value?.ToString());
                    }
                }
            }

            _context.Add(entity);
            await _context.SaveChangesAsync();

            return ToLightweightObject(entity, partType);
        }

        public async Task<object?> UpdateAsync(string partType, int id, Dictionary<string, object> data)
        {
            var modelType = GetModelType(partType);
            if (modelType == null)
                throw new ArgumentException($"Invalid part type: {partType}");

            var entity = await _context.FindAsync(modelType, id);
            if (entity == null)
                return null;

            // Update properties from data dictionary
            foreach (var kvp in data)
            {
                var property = modelType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null && property.CanWrite && kvp.Key.ToLower() != "id")
                {
                    try
                    {
                        var value = Convert.ChangeType(kvp.Value, property.PropertyType);
                        property.SetValue(entity, value);
                    }
                    catch
                    {
                        property.SetValue(entity, kvp.Value?.ToString());
                    }
                }
            }

            _context.Update(entity);
            await _context.SaveChangesAsync();

            return ToLightweightObject(entity, partType);
        }

        public async Task<bool> DeleteAsync(string partType, int id)
        {
            var modelType = GetModelType(partType);
            if (modelType == null)
                throw new ArgumentException($"Invalid part type: {partType}");

            var entity = await _context.FindAsync(modelType, id);
            if (entity == null)
                return false;

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<SearchResult> SearchAsync(string partType, Dictionary<string, string> filters)
        {
            var normalizedPartType = partType.ToLower();
            return normalizedPartType switch
            {
                "cpu" or "cpus" => await SearchAsyncInternal(_context.Cpus, "cpu", filters),
                "gpu" or "gpus" => await SearchAsyncInternal(_context.Gpus, "gpu", filters),
                "memory" or "memories" => await SearchAsyncInternal(_context.Memories, "memory", filters),
                "motherboard" or "motherboards" => await SearchAsyncInternal(_context.Motherboards, "motherboard", filters),
                "case" or "cases" => await SearchAsyncInternal(_context.Cases, "case", filters),
                "storage" or "storages" => await SearchAsyncInternal(_context.Storages, "storage", filters),
                "powersupply" or "powersupplies" => await SearchAsyncInternal(_context.PowerSupplies, "powersupply", filters),
                "cpucooler" or "cpucoolers" => await SearchAsyncInternal(_context.CpuCoolers, "cpucooler", filters),
                _ => throw new ArgumentException($"Invalid part type: {partType}")
            };
        }

        private async Task<SearchResult> SearchAsyncInternal<T>(DbSet<T> dbSet, string partType, Dictionary<string, string> filters) where T : class
        {
            decimal? minPrice = null;
            decimal? maxPrice = null;
            var appliedFilters = new Dictionary<string, string>();

            // Build IQueryable with expression-based filters (pushed to SQL)
            IQueryable<T> query = dbSet;

            foreach (var filter in filters)
            {
                var key = filter.Key.ToLower();
                var value = filter.Value;

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                appliedFilters[filter.Key] = value;

                if (key == "minprice")
                {
                    if (decimal.TryParse(value, out var min))
                        minPrice = min;
                    continue;
                }

                if (key == "maxprice")
                {
                    if (decimal.TryParse(value, out var max))
                        maxPrice = max;
                    continue;
                }

                // String property filter - EF Core translates to SQL
                var property = typeof(T).GetProperty(filter.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null && property.PropertyType == typeof(string))
                {
                    var containsExpr = BuildContainsExpression<T>(property, value);
                    query = query.Where(containsExpr);
                }
            }

            // Execute query - only matching rows are loaded from DB
            var items = await query.ToListAsync();

            // Apply price filter in memory (Price stored as string like "$599.99")
            if (minPrice.HasValue || maxPrice.HasValue)
            {
                var priceProperty = typeof(T).GetProperty("Price", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (priceProperty != null)
                {
                    items = items.Where(item =>
                    {
                        var priceValue = priceProperty.GetValue(item)?.ToString();
                        if (string.IsNullOrEmpty(priceValue))
                            return false;

                        var cleanPrice = priceValue.Replace("$", "").Replace(",", "").Trim();
                        if (decimal.TryParse(cleanPrice, out var price))
                        {
                            if (minPrice.HasValue && price < minPrice.Value)
                                return false;
                            if (maxPrice.HasValue && price > maxPrice.Value)
                                return false;
                            return true;
                        }
                        return false;
                    }).ToList();
                }
            }

            var lightweightResults = items.Select(item => ToLightweightObject(item, partType)).ToList();

            // Average part: check cache first, then calculate and save
            object? averagePart = null;
            if (lightweightResults.Count > 0)
            {
                var filterKey = BuildFilterKey(partType, appliedFilters);
                var cached = await _context.PartAverageCaches
                    .FirstOrDefaultAsync(c => c.PartType == partType && c.FilterKey == filterKey);

                if (cached != null)
                {
                    averagePart = JsonSerializer.Deserialize<JsonElement>(cached.AveragePartJson);
                }
                else
                {
                    averagePart = CalculateAveragePart(lightweightResults, partType);
                    await SaveAveragePartToCacheAsync(partType, filterKey, averagePart);
                }
            }

            return new SearchResult
            {
                PartType = partType,
                TotalCount = lightweightResults.Count,
                Results = lightweightResults,
                AppliedFilters = appliedFilters,
                AveragePart = averagePart
            };
        }

        private static Expression<Func<T, bool>> BuildContainsExpression<T>(PropertyInfo property, string value)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var propExpr = Expression.Property(param, property);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            if (containsMethod == null)
                throw new InvalidOperationException("string.Contains not found");

            var containsCall = Expression.Call(propExpr, containsMethod, Expression.Constant(value));
            var nullCheck = Expression.NotEqual(propExpr, Expression.Constant(null, typeof(string)));
            var combined = Expression.AndAlso(nullCheck, containsCall);
            return Expression.Lambda<Func<T, bool>>(combined, param);
        }

        private static string BuildFilterKey(string partType, Dictionary<string, string> filters)
        {
            var sorted = filters.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
            var keyString = string.Join(";", sorted.Select(kvp => $"{kvp.Key.ToLowerInvariant()}={kvp.Value}"));
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{partType}:{keyString}"));
            return Convert.ToHexString(bytes)[..64];
        }

        private async Task SaveAveragePartToCacheAsync(string partType, string filterKey, object averagePart)
        {
            try
            {
                var json = JsonSerializer.Serialize(averagePart);
                var cache = new PartAverageCache
                {
                    PartType = partType,
                    FilterKey = filterKey,
                    AveragePartJson = json
                };
                _context.PartAverageCaches.Add(cache);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save average part to cache for {PartType} with filter key {FilterKey}", partType, filterKey);
            }
        }
        
        private object CalculateAveragePart(List<object> parts, string partType)
        {
            var averageData = new Dictionary<string, object>
            {
                ["Name"] = "Average Part",
                ["Manufacturer"] = "Statistical Summary",
                ["PartNumber"] = $"Based on {parts.Count} parts"
            };
            
            // Calculate average price
            decimal totalPrice = 0;
            int priceCount = 0;
            foreach (var part in parts)
            {
                // Handle both Dictionary and ExpandoObject/IDictionary
                var dict = part as IDictionary<string, object>;
                if (dict != null && dict.TryGetValue("Price", out var priceObj))
                {
                    var priceStr = priceObj?.ToString()?.Replace("$", "").Replace(",", "").Trim();
                    if (decimal.TryParse(priceStr, out var price) && price > 0)
                    {
                        totalPrice += price;
                        priceCount++;
                    }
                }
            }
            
            if (priceCount > 0)
            {
                averageData["Price"] = "$" + (totalPrice / priceCount).ToString("F2");
            }
            else
            {
                averageData["Price"] = "N/A";
            }
            
            // Count attribute occurrences
            var attributeCounts = new Dictionary<string, Dictionary<string, int>>();
            var excludeFields = new HashSet<string> { "Id", "Name", "ImageUrl", "ProductUrl", "Price", "Manufacturer", "PartNumber", "PartType", "SpecsNumber" };
            
            foreach (var part in parts)
            {
                // Handle both Dictionary and ExpandoObject/IDictionary
                var dict = part as IDictionary<string, object>;
                if (dict == null) continue;
                
                foreach (var kvp in dict)
                {
                    if (excludeFields.Contains(kvp.Key)) continue;
                    if (kvp.Value == null || string.IsNullOrWhiteSpace(kvp.Value.ToString())) continue;
                    
                    if (!attributeCounts.ContainsKey(kvp.Key))
                    {
                        attributeCounts[kvp.Key] = new Dictionary<string, int>();
                    }
                    
                    var value = kvp.Value.ToString()!;
                    if (!attributeCounts[kvp.Key].ContainsKey(value))
                    {
                        attributeCounts[kvp.Key][value] = 0;
                    }
                    attributeCounts[kvp.Key][value]++;
                }
            }
            
            // Find most common value for each attribute
            foreach (var attr in attributeCounts)
            {
                var mostCommon = attr.Value.OrderByDescending(v => v.Value).First();
                var percentage = ((double)mostCommon.Value / parts.Count * 100).ToString("F0");
                averageData[attr.Key] = $"{mostCommon.Key} ({percentage}%)";
            }
            
            return averageData;
        }

        public async Task<FilterOptions> GetFilterOptionsAsync(string partType)
        {
            var normalizedPartType = partType.ToLower();
            return normalizedPartType switch
            {
                "cpu" or "cpus" => await GetFilterOptionsInternal(_context.Cpus, "cpu"),
                "gpu" or "gpus" => await GetFilterOptionsInternal(_context.Gpus, "gpu"),
                "memory" or "memories" => await GetFilterOptionsInternal(_context.Memories, "memory"),
                "motherboard" or "motherboards" => await GetFilterOptionsInternal(_context.Motherboards, "motherboard"),
                "case" or "cases" => await GetFilterOptionsInternal(_context.Cases, "case"),
                "storage" or "storages" => await GetFilterOptionsInternal(_context.Storages, "storage"),
                "powersupply" or "powersupplies" => await GetFilterOptionsInternal(_context.PowerSupplies, "powersupply"),
                "cpucooler" or "cpucoolers" => await GetFilterOptionsInternal(_context.CpuCoolers, "cpucooler"),
                _ => throw new ArgumentException($"Invalid part type: {partType}")
            };
        }

        private async Task<FilterOptions> GetFilterOptionsInternal<T>(DbSet<T> dbSet, string partType) where T : class
        {
            var allItems = await dbSet.ToListAsync();
            var modelType = typeof(T);
            var filterOptions = new FilterOptions
            {
                PartType = partType,
                Attributes = new Dictionary<string, FilterAttribute>()
            };

            var properties = modelType.GetProperties()
                .Where(p => p.Name != "Id" &&
                           p.Name != "ImageUrl" &&
                           p.Name != "ProductUrl" &&
                           p.Name != "PartNumber" &&
                           p.CanRead)
                .ToList();

            foreach (var property in properties)
            {
                try
                {
                    var distinctValues = allItems
                        .Select(item => property.GetValue(item))
                        .Where(value => value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                        .Select(value => value!.ToString()!)
                        .Distinct()
                        .OrderBy(v => v)
                        .ToList();

                    if (distinctValues.Count > 0)
                    {
                        filterOptions.Attributes[property.Name] = new FilterAttribute
                        {
                            AttributeName = property.Name,
                            AttributeType = GetFriendlyTypeName(property.PropertyType),
                            DistinctValues = distinctValues,
                            TotalDistinctCount = distinctValues.Count
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting distinct values for property {PropertyName}", property.Name);
                }
            }

            return filterOptions;
        }

        private string GetFriendlyTypeName(Type type)
        {
            // Handle nullable types
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type)!;
            }

            return type.Name switch
            {
                "String" => "string",
                "Int32" => "integer",
                "Decimal" => "decimal",
                "Boolean" => "boolean",
                "DateTime" => "datetime",
                _ => type.Name.ToLower()
            };
        }
    }
}

