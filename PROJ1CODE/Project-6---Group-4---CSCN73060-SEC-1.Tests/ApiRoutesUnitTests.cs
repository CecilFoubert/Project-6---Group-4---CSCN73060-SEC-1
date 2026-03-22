using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Project_6___Group_4___CSCN73060_SEC_1.Tests;

public class ApiRoutesUnitTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiRoutesUnitTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Options_ReturnsRoutesList()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/options");
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("routes", out var routes));
        Assert.True(routes.GetArrayLength() > 0);
    }

    [Fact]
    public async Task Parts_GetSupportedTypes_ReturnsPartTypes()
    {
        var response = await _client.GetAsync("/api/Parts");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("types", out var types));
        var typesArray = types.EnumerateArray().Select(t => t.GetString()).ToList();
        Assert.Contains("cpu", typesArray);
        Assert.Contains("gpu", typesArray);
    }

    [Fact]
    public async Task Parts_GetAllCpus_ReturnsCpus()
    {
        var response = await _client.GetAsync("/api/Parts/cpu");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("cpu", json.GetProperty("partType").GetString());
        Assert.True(json.TryGetProperty("data", out var data));
        Assert.True(data.GetArrayLength() >= 3); // TestDbSeeder adds 3 CPUs
    }

    [Fact]
    public async Task Parts_GetById_ReturnsCpu()
    {
        var response = await _client.GetAsync("/api/Parts/cpu/1");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, json.GetProperty("Id").GetInt32());
        Assert.True(!string.IsNullOrEmpty(json.GetProperty("Name").GetString()));
    }

    [Fact]
    public async Task Parts_GetById_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/Parts/cpu/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Parts_GetFilters_ReturnsFilterOptions()
    {
        var response = await _client.GetAsync("/api/Parts/cpu/filters");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("cpu", json.GetProperty("partType").GetString());
        Assert.True(json.TryGetProperty("attributes", out var attrs));
    }

    [Fact]
    public async Task Parts_SearchCpus_ReturnsFilteredResults()
    {
        var searchBody = new
        {
            minPrice = 100m,
            maxPrice = 300m,
            manufacturer = "Intel",
            filters = new Dictionary<string, string> { ["Socket"] = "LGA1700" }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(searchBody),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/Parts/cpu/search", content);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("cpu", json.GetProperty("partType").GetString());
        Assert.True(json.TryGetProperty("results", out var results));
        Assert.True(json.TryGetProperty("totalCount", out var totalCount));
        // Should find Intel CPUs in price range (e.g. i5-12400 at 199.99, i7-13700K at 409.99 - i7 is out of range)
        var count = totalCount.GetInt32();
        Assert.True(count >= 1);
    }

    [Fact]
    public async Task Parts_Create_ReturnsCreated()
    {
        var partData = new Dictionary<string, object>
        {
            ["Name"] = "Test CPU",
            ["Price"] = "99.99",
            ["Manufacturer"] = "TestBrand"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(partData),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/Parts/cpu", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("Id", out _));
    }

    [Fact]
    public async Task Parts_Update_ReturnsOk()
    {
        var partData = new Dictionary<string, object>
        {
            ["Name"] = "Updated CPU Name",
            ["Price"] = "149.99",
            ["Manufacturer"] = "Intel"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(partData),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PutAsync("/api/Parts/cpu/1", content);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Parts_Delete_ReturnsOk()
    {
        // Create a part first to delete
        var createData = new Dictionary<string, object>
        {
            ["Name"] = "To Delete CPU",
            ["Price"] = "50.00",
            ["Manufacturer"] = "Test"
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createData),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/Parts/cpu", createContent);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("Id").GetInt32();

        var response = await _client.DeleteAsync($"/api/Parts/cpu/{id}");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Builds_GetAll_ReturnsBuilds()
    {
        var response = await _client.GetAsync("/api/Builds");

        response.EnsureSuccessStatusCode();
        var builds = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(builds.ValueKind == JsonValueKind.Array);
    }

    [Fact]
    public async Task Builds_Create_ReturnsCreated()
    {
        var buildDto = new
        {
            Name = "Test Build",
            Description = "A test build",
            Parts = new Dictionary<string, object>
            {
                ["cpu"] = new { Id = 1 }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(buildDto),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/Builds", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("id", out _) || json.TryGetProperty("Id", out _));
    }

    [Fact]
    public async Task Builds_GetById_ReturnsBuild()
    {
        // Create a build first
        var createDto = new { Name = "GetById Test Build", Description = "" };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createDto),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/Builds", createContent);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : created.GetProperty("Id").GetInt32();

        var response = await _client.GetAsync($"/api/Builds/{id}");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("GetById Test Build", json.GetProperty("name").GetString() ?? json.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task Builds_GetById_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/Builds/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Builds_GetByName_ReturnsBuild()
    {
        var uniqueName = $"ByName Test {Guid.NewGuid():N}";
        var createDto = new { Name = uniqueName, Description = "" };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createDto),
            Encoding.UTF8,
            "application/json");
        await _client.PostAsync("/api/Builds", createContent);

        var response = await _client.GetAsync($"/api/Builds/by-name/{Uri.EscapeDataString(uniqueName)}");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(uniqueName, json.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Builds_GetByName_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/Builds/by-name/NonExistentBuildName12345");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Builds_Patch_ReturnsOk()
    {
        var createDto = new { Name = "Patch Test Build", Description = "Original" };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createDto),
            Encoding.UTF8,
            "application/json");
        var createResponse = await _client.PostAsync("/api/Builds", createContent);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : created.GetProperty("Id").GetInt32();

        var patchDto = new { Name = "Patch Test Build", Description = "Updated description" };
        var patchContent = new StringContent(
            JsonSerializer.Serialize(patchDto),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PatchAsync($"/api/Builds/{id}", patchContent);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Updated description", json.GetProperty("description").GetString());
    }
}
