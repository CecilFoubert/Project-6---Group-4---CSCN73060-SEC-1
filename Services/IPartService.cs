namespace Project_6___Group_4___CSCN73060_SEC_1.Services
{
    public interface IPartService
    {
        Task<IEnumerable<object>> GetAllAsync(string partType);
        Task<object?> GetByIdAsync(string partType, int id);
        Task<object> CreateAsync(string partType, Dictionary<string, object> data);
        Task<object?> UpdateAsync(string partType, int id, Dictionary<string, object> data);
        Task<bool> DeleteAsync(string partType, int id);
        Task<SearchResult> SearchAsync(string partType, Dictionary<string, string> filters);
        Task<FilterOptions> GetFilterOptionsAsync(string partType);
    }

    public class SearchResult
    {
        public string PartType { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public IEnumerable<object> Results { get; set; } = new List<object>();
        public Dictionary<string, string> AppliedFilters { get; set; } = new Dictionary<string, string>();
    }

    public class FilterOptions
    {
        public string PartType { get; set; } = string.Empty;
        public Dictionary<string, FilterAttribute> Attributes { get; set; } = new Dictionary<string, FilterAttribute>();
    }

    public class FilterAttribute
    {
        public string AttributeName { get; set; } = string.Empty;
        public string AttributeType { get; set; } = string.Empty;
        public List<string> DistinctValues { get; set; } = new List<string>();
        public int TotalDistinctCount { get; set; }
    }
}

