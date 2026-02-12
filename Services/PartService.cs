using Microsoft.EntityFrameworkCore;
using Project_6___Group_4___CSCN73060_SEC_1.Data;
using Project_6___Group_4___CSCN73060_SEC_1.Models;
using System.Dynamic;
using System.Reflection;

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

        private DbSet<T>? GetDbSet<T>(string partType) where T : class
        {
            return partType.ToLower() switch
            {
                "cpu" or "cpus" => _context.Set<T>(),
                "gpu" or "gpus" => _context.Set<T>(),
                "memory" or "memories" => _context.Set<T>(),
                "motherboard" or "motherboards" => _context.Set<T>(),
                "case" or "cases" => _context.Set<T>(),
                "storage" or "storages" => _context.Set<T>(),
                "powersupply" or "powersupplies" => _context.Set<T>(),
                "cpucooler" or "cpucoolers" => _context.Set<T>(),
                _ => null
            };
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
            var modelType = GetModelType(partType);
            if (modelType == null)
                throw new ArgumentException($"Invalid part type: {partType}");

            var dbSetProperty = _context.GetType().GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                   p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                                   p.PropertyType.GetGenericArguments()[0] == modelType);

            if (dbSetProperty == null)
                throw new ArgumentException($"DbSet not found for type: {partType}");

            var dbSet = dbSetProperty.GetValue(_context);
            var queryable = dbSet as IQueryable<object>;
            
            if (queryable == null)
            {
                var castMethod = typeof(Queryable).GetMethod("Cast")!.MakeGenericMethod(typeof(object));
                queryable = (IQueryable<object>)castMethod.Invoke(null, new[] { dbSet })!;
            }

            var items = await queryable.ToListAsync();
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
            var modelType = GetModelType(partType);
            if (modelType == null)
                throw new ArgumentException($"Invalid part type: {partType}");

            var dbSetProperty = _context.GetType().GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                   p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                                   p.PropertyType.GetGenericArguments()[0] == modelType);

            if (dbSetProperty == null)
                throw new ArgumentException($"DbSet not found for type: {partType}");

            var dbSet = dbSetProperty.GetValue(_context);
            var queryable = dbSet as IQueryable<object>;
            
            if (queryable == null)
            {
                var castMethod = typeof(Queryable).GetMethod("Cast")!.MakeGenericMethod(typeof(object));
                queryable = (IQueryable<object>)castMethod.Invoke(null, new[] { dbSet })!;
            }

            // Convert to list first to enable in-memory filtering for complex queries
            var allItems = await queryable.ToListAsync();
            var filteredItems = allItems.AsEnumerable();

            // Extract price range filters
            decimal? minPrice = null;
            decimal? maxPrice = null;
            var appliedFilters = new Dictionary<string, string>();

            foreach (var filter in filters)
            {
                var key = filter.Key.ToLower();
                var value = filter.Value;

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                appliedFilters[filter.Key] = value;

                // Handle price range
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

                // Handle other filters dynamically
                var property = modelType.GetProperty(filter.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    filteredItems = filteredItems.Where(item =>
                    {
                        var propValue = property.GetValue(item);
                        if (propValue == null)
                            return false;

                        var propStringValue = propValue.ToString();
                        if (string.IsNullOrEmpty(propStringValue))
                            return false;

                        // Case-insensitive partial match
                        return propStringValue.Contains(value, StringComparison.OrdinalIgnoreCase);
                    });
                }
            }

            // Apply price range filter
            if (minPrice.HasValue || maxPrice.HasValue)
            {
                var priceProperty = modelType.GetProperty("Price", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (priceProperty != null)
                {
                    filteredItems = filteredItems.Where(item =>
                    {
                        var priceValue = priceProperty.GetValue(item)?.ToString();
                        if (string.IsNullOrEmpty(priceValue))
                            return false;

                        // Try to parse price (handle formats like "$599.99" or "599.99")
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
                    });
                }
            }

            var resultList = filteredItems.ToList();
            var lightweightResults = resultList.Select(item => ToLightweightObject(item, partType)).ToList();

            return new SearchResult
            {
                PartType = partType,
                TotalCount = lightweightResults.Count,
                Results = lightweightResults,
                AppliedFilters = appliedFilters
            };
        }

        public async Task<FilterOptions> GetFilterOptionsAsync(string partType)
        {
            var modelType = GetModelType(partType);
            if (modelType == null)
                throw new ArgumentException($"Invalid part type: {partType}");

            var dbSetProperty = _context.GetType().GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                   p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                                   p.PropertyType.GetGenericArguments()[0] == modelType);

            if (dbSetProperty == null)
                throw new ArgumentException($"DbSet not found for type: {partType}");

            var dbSet = dbSetProperty.GetValue(_context);
            var queryable = dbSet as IQueryable<object>;
            
            if (queryable == null)
            {
                var castMethod = typeof(Queryable).GetMethod("Cast")!.MakeGenericMethod(typeof(object));
                queryable = (IQueryable<object>)castMethod.Invoke(null, new[] { dbSet })!;
            }

            var allItems = await queryable.ToListAsync();

            var filterOptions = new FilterOptions
            {
                PartType = partType,
                Attributes = new Dictionary<string, FilterAttribute>()
            };

            // Get all properties except ones we don't want to filter by
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
                    // Get all non-null, non-empty values for this property
                    var distinctValues = allItems
                        .Select(item => property.GetValue(item))
                        .Where(value => value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                        .Select(value => value!.ToString()!)
                        .Distinct()
                        .OrderBy(v => v)
                        .ToList();

                    if (distinctValues.Any())
                    {
                        var attributeName = property.Name;
                        var attributeType = GetFriendlyTypeName(property.PropertyType);

                        filterOptions.Attributes[attributeName] = new FilterAttribute
                        {
                            AttributeName = attributeName,
                            AttributeType = attributeType,
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

