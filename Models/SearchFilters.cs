namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    /// <summary>
    /// Model for search filters sent as JSON body to the search endpoint
    /// </summary>
    public class SearchFilters
    {
        /// <summary>
        /// Minimum price filter (inclusive)
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Maximum price filter (inclusive)
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Manufacturer filter (partial match, case-insensitive)
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Dynamic filters as key-value pairs for any part property
        /// Keys are property names (e.g., "Socket", "Chipset", "Speed")
        /// Values are filter values (partial match, case-insensitive)
        /// </summary>
        public Dictionary<string, string>? Filters { get; set; }
    }
}
