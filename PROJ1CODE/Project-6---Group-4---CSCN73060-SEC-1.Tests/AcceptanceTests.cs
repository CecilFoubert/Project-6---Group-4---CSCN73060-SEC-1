using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Project_6___Group_4___CSCN73060_SEC_1.Tests;

/// <summary>
/// Acceptance tests verifying end-to-end user workflows and business scenarios.
/// </summary>
public class AcceptanceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AcceptanceTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task User_Can_BrowseCpus_And_CreateBuild_WithSelectedCpu()
    {
        // Given: User wants to build a PC
        // When: User browses available CPUs and selects one for their build
        var cpusResponse = await _client.GetAsync("/api/Parts/cpu");
        cpusResponse.EnsureSuccessStatusCode();
        var cpusJson = await cpusResponse.Content.ReadFromJsonAsync<JsonElement>();
        var cpus = cpusJson.GetProperty("data");
        var firstCpuId = cpus[0].GetProperty("Id").GetInt32();

        var buildDto = new
        {
            Name = "My Gaming PC",
            Description = "Budget gaming build",
            Parts = new Dictionary<string, object> { ["cpu"] = new { Id = firstCpuId } }
        };

        var createResponse = await _client.PostAsync("/api/Builds",
            new StringContent(JsonSerializer.Serialize(buildDto), Encoding.UTF8, "application/json"));

        // Then: Build is created with the selected CPU
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var buildId = created.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : created.GetProperty("Id").GetInt32();

        var getResponse = await _client.GetAsync($"/api/Builds/{buildId}");
        getResponse.EnsureSuccessStatusCode();
        var build = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("My Gaming PC", build.GetProperty("name").GetString());
        Assert.True(build.TryGetProperty("parts", out var parts) && parts.TryGetProperty("cpu", out _));
    }

    [Fact]
    public async Task User_Can_SearchCpus_ByPriceAndManufacturer_ThenCreateBuild()
    {
        // Given: User has a budget and prefers Intel
        // When: User searches for Intel CPUs under $300
        var searchBody = new
        {
            minPrice = 100m,
            maxPrice = 300m,
            manufacturer = "Intel"
        };
        var searchResponse = await _client.PostAsync("/api/Parts/cpu/search",
            new StringContent(JsonSerializer.Serialize(searchBody), Encoding.UTF8, "application/json"));
        searchResponse.EnsureSuccessStatusCode();

        var searchResult = await searchResponse.Content.ReadFromJsonAsync<JsonElement>();
        var results = searchResult.GetProperty("results");
        Assert.True(results.GetArrayLength() >= 1);

        var selectedCpuId = results[0].GetProperty("Id").GetInt32();

        var buildDto = new
        {
            Name = "Intel Budget Build",
            Description = "",
            Parts = new Dictionary<string, object> { ["cpu"] = new { Id = selectedCpuId } }
        };

        var createResponse = await _client.PostAsync("/api/Builds",
            new StringContent(JsonSerializer.Serialize(buildDto), Encoding.UTF8, "application/json"));

        // Then: Build is created with a CPU matching the search criteria
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var buildId = created.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : created.GetProperty("Id").GetInt32();

        var getResponse = await _client.GetAsync($"/api/Builds/{buildId}");
        getResponse.EnsureSuccessStatusCode();
        var build = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var cpuPart = build.GetProperty("parts").GetProperty("cpu");
        Assert.True(cpuPart.GetProperty("Manufacturer").GetString()!.Contains("Intel", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task User_Can_CreateBuild_SaveWithName_AndRetrieveLater()
    {
        // Given: User creates a new build
        var buildName = $"Saved Build {Guid.NewGuid():N}";
        var createDto = new { Name = buildName, Description = "My saved configuration" };
        var createResponse = await _client.PostAsync("/api/Builds",
            new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json"));
        createResponse.EnsureSuccessStatusCode();

        // When: User retrieves the build by name later
        var getByNameResponse = await _client.GetAsync($"/api/Builds/by-name/{Uri.EscapeDataString(buildName)}");

        // Then: User gets their saved build
        getByNameResponse.EnsureSuccessStatusCode();
        var build = await getByNameResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(buildName, build.GetProperty("name").GetString());
        Assert.Equal("My saved configuration", build.GetProperty("description").GetString());
    }

    [Fact]
    public async Task User_Can_UpdateExistingBuild_WhenSavingWithSameName()
    {
        // Given: User has an existing build named "My Build"
        var buildName = "My Build";
        var createDto = new { Name = buildName, Description = "Original description", Parts = (object?)null };
        var createResponse = await _client.PostAsync("/api/Builds",
            new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json"));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var buildId = created.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : created.GetProperty("Id").GetInt32();

        // When: User saves again with the same name but updated description and parts
        var updateDto = new
        {
            Name = buildName,
            Description = "Updated - added CPU",
            Parts = new Dictionary<string, object> { ["cpu"] = new { Id = 1 } }
        };
        var patchResponse = await _client.PatchAsync($"/api/Builds/{buildId}",
            new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json"));

        // Then: Build is updated in place
        patchResponse.EnsureSuccessStatusCode();
        var updated = await patchResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Updated - added CPU", updated.GetProperty("description").GetString());
        Assert.True(updated.TryGetProperty("parts", out var parts) && parts.TryGetProperty("cpu", out _));
    }

    [Fact]
    public async Task User_Can_GetFilterOptions_UseThemToSearch_AndViewResults()
    {
        // Given: User wants to filter CPUs
        // When: User fetches available filter options
        var filtersResponse = await _client.GetAsync("/api/Parts/cpu/filters");
        filtersResponse.EnsureSuccessStatusCode();
        var filtersJson = await filtersResponse.Content.ReadFromJsonAsync<JsonElement>();
        var attributes = filtersJson.GetProperty("attributes");

        // Then: User can use filter attributes (e.g. Manufacturer, Socket) to search
        Assert.True(attributes.EnumerateObject().Any());

        var searchBody = new
        {
            minPrice = (decimal?)null,
            maxPrice = (decimal?)null,
            manufacturer = "AMD",
            filters = new Dictionary<string, string>()
        };
        var searchResponse = await _client.PostAsync("/api/Parts/cpu/search",
            new StringContent(JsonSerializer.Serialize(searchBody), Encoding.UTF8, "application/json"));
        searchResponse.EnsureSuccessStatusCode();

        var searchResult = await searchResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("cpu", searchResult.GetProperty("partType").GetString());
        Assert.True(searchResult.TryGetProperty("results", out var results));
        // TestDbSeeder has AMD Ryzen 5 5600X
        Assert.True(results.GetArrayLength() >= 1);
    }
}
