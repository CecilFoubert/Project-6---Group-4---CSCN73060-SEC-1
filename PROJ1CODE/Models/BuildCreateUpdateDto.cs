/*!
 * @file Models/BuildCreateUpdateDto.cs
 * @brief DTO used by the API for creating/updating builds.
 * @ingroup Models
 */

namespace Project_6___Group_4___CSCN73060_SEC_1.Models
{
    /// <summary>
    /// DTO for creating or updating a build from the frontend.
    /// Parts object maps part types to part IDs (e.g., { "cpu": { "Id": 5 }, "gpu": { "Id": 10 } }).
    /// </summary>
    public class BuildCreateUpdateDto
    {
        /// <summary>
        /// The build name provided by the user. Required for creation.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description for the build.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Mapping of part type keys to selected part reference (Id).
        /// Example: { "cpu": { "Id": 5 }, "gpu": { "Id": 10 } }.
        /// </summary>
        public Dictionary<string, PartRefDto>? Parts { get; set; }
    }

    /// <summary>
    /// Simple reference to a part by id used in BuildCreateUpdateDto.
    /// </summary>
    public class PartRefDto
    {
        /// <summary>
        /// Database id of the referenced part.
        /// </summary>
        public int Id { get; set; }
    }
}
