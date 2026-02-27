/*!
 * @file Services/IPartService.cs
 * @brief Interface definitions for part-related operations.
 * @ingroup Services
 */

namespace Project_6___Group_4___CSCN73060_SEC_1.Services
{
    /// <summary>
    /// Service interface for retrieving and manipulating parts of different types.
    /// Implementations provide a unified abstraction over multiple DbSets (CPU, GPU, Memory, etc.).
    /// </summary>
    public interface IPartService
    {
        /// <summary>
        /// Get all parts for the given part type (lightweight API representation).
        /// </summary>
        Task<IEnumerable<object>> GetAllAsync(string partType);

        /// <summary>
        /// Get a single part by id for the specified part type.
        /// Returns null if not found.
        /// </summary>
        Task<object?> GetByIdAsync(string partType, int id);

        /// <summary>
        /// Create a new part for the given part type using the provided data dictionary.
        /// Returns the created lightweight representation.
        /// </summary>
        Task<object> CreateAsync(string partType, Dictionary<string, object> data);

        /// <summary>
        /// Update an existing part. Returns the updated lightweight object or null when not found.
        /// </summary>
        Task<object?> UpdateAsync(string partType, int id, Dictionary<string, object> data);

        /// <summary>
        /// Delete a part. Returns true when the item was removed.
        /// </summary>
        Task<bool> DeleteAsync(string partType, int id);

        /// <summary>
        /// Search parts of a given type using a set of string filters (minPrice/maxPrice/manufacturer/attributes).
        /// Returns a <see cref="SearchResult"/> containing results and summary info.
        /// </summary>
        Task<SearchResult> SearchAsync(string partType, Dictionary<string, string> filters);

        /// <summary>
        /// Get distinct filter options for a given part type (attributes and their distinct values).
        /// </summary>
        Task<FilterOptions> GetFilterOptionsAsync(string partType);
    }

    /// <summary>
    /// Container for search results returned by <see cref="IPartService.SearchAsync"/>.
    /// </summary>
    public class SearchResult
    {
        public string PartType { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public IEnumerable<object> Results { get; set; } = new List<object>();
        public Dictionary<string, string> AppliedFilters { get; set; } = new Dictionary<string, string>();
        public object? AveragePart { get; set; }
    }

    /// <summary>
    /// Describes the available filter options for a single part type.
    /// </summary>
    public class FilterOptions
    {
        public string PartType { get; set; } = string.Empty;
        public Dictionary<string, FilterAttribute> Attributes { get; set; } = new Dictionary<string, FilterAttribute>();
    }

    /// <summary>
    /// Information about a single filterable attribute: name, type and distinct values.
    /// </summary>
    public class FilterAttribute
    {
        public string AttributeName { get; set; } = string.Empty;
        public string AttributeType { get; set; } = string.Empty;
        public List<string> DistinctValues { get; set; } = new List<string>();
        public int TotalDistinctCount { get; set; }
    }
}

