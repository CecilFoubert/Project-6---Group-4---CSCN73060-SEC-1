namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    /// <summary>
    /// DTO for creating or updating a build from the frontend.
    /// Parts object maps part types to part IDs (e.g., { "cpu": { "Id": 5 }, "gpu": { "Id": 10 } }).
    /// </summary>
    public class BuildCreateUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Dictionary<string, PartRefDto>? Parts { get; set; }
    }

    public class PartRefDto
    {
        public int Id { get; set; }
    }
}
