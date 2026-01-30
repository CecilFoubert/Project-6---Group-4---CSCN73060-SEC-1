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

            result["PartType"] = partType;
            result["Id"] = properties.FirstOrDefault(p => p.Name == "Id")?.GetValue(entity);
            result["Name"] = properties.FirstOrDefault(p => p.Name == "Name")?.GetValue(entity);
            result["Price"] = properties.FirstOrDefault(p => p.Name == "Price")?.GetValue(entity);
            result["Manufacturer"] = properties.FirstOrDefault(p => p.Name == "Manufacturer")?.GetValue(entity);
            result["ImageUrl"] = properties.FirstOrDefault(p => p.Name == "ImageUrl")?.GetValue(entity);

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
    }
}

