using Craft.IntegrationTests.Fixtures;
using Craft.MultiTenant;
using System.Net;
using System.Net.Http.Json;

namespace Craft.IntegrationTests.Http;

/// <summary>
/// Integration tests for HTTP pipeline including middleware, controllers, and multi-tenant resolution.
/// </summary>
[Collection(nameof(HttpTestCollection))]
public class HttpPipelineIntegrationTests
{
    private readonly TestHostFixture _fixture;

    public HttpPipelineIntegrationTests(TestHostFixture fixture)
    {
        _fixture = fixture;
    }

    #region Basic HTTP Tests

    [Fact]
    public async Task WeatherForecast_Get_ReturnsSuccess()
    {
        // Arrange
        var client = _fixture.Client;

        // Act
        var response = await client.GetAsync("/WeatherForecast");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Tenant Resolution HTTP Tests

    [Fact]
    public async Task TenantEndpoint_WithValidTenantHeader_ReturnsTenantInfo()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("alpha");

        // Act
        var response = await client.GetAsync("/api/Tenant");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tenantInfo = await response.Content.ReadFromJsonAsync<TenantInfoResponse>();
        Assert.NotNull(tenantInfo);
        Assert.Equal("Alpha Corp", tenantInfo.Name);
        Assert.Equal("alpha", tenantInfo.Identifier);
        Assert.True(tenantInfo.IsActive);
    }

    [Fact]
    public async Task TenantEndpoint_WithInvalidTenantHeader_ReturnsNotFound()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("nonexistent");

        // Act
        var response = await client.GetAsync("/api/Tenant");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TenantEndpoint_WithoutTenantHeader_ReturnsNotFound()
    {
        // Arrange
        var client = _fixture.Client;

        // Act
        var response = await client.GetAsync("/api/Tenant");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TenantCheck_WithValidTenant_ReturnsTrue()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("beta");

        // Act
        var response = await client.GetAsync("/api/Tenant/check");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var isAvailable = await response.Content.ReadFromJsonAsync<bool>();
        Assert.True(isAvailable);
    }

    [Fact]
    public async Task TenantCheck_WithoutTenant_ReturnsFalse()
    {
        // Arrange
        var client = _fixture.Client;

        // Act
        var response = await client.GetAsync("/api/Tenant/check");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var isAvailable = await response.Content.ReadFromJsonAsync<bool>();
        Assert.False(isAvailable);
    }

    #endregion

    #region Product Controller HTTP Tests

    [Fact]
    public async Task Products_GetAll_ReturnsProducts()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("alpha");

        // Act
        var response = await client.GetAsync("/api/Products/true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Products_GetById_ReturnsProduct()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("alpha");

        // Act
        var response = await client.GetAsync("/api/Products/1/true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
        Assert.Equal("Laptop", product.Name);
    }

    [Fact]
    public async Task Products_GetById_NotFound_ReturnsNotFound()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("alpha");

        // Act
        var response = await client.GetAsync("/api/Products/99999/true");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Products_GetCount_ReturnsCount()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("alpha");

        // Act
        var response = await client.GetAsync("/api/Products/count");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var count = await response.Content.ReadFromJsonAsync<long>();
        Assert.True(count > 0);
    }

    [Fact]
    public async Task Products_Create_ReturnsCreatedProduct()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("alpha");
        var newProduct = new CreateProductRequest
        {
            Name = "New HTTP Product " + Guid.NewGuid(),
            Price = 149.99m,
            Description = "Created via HTTP"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Products", newProduct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        Assert.Contains("New HTTP Product", created.Name);
        Assert.NotEqual(default, created.Id);
    }

    [Fact]
    public async Task Products_Delete_ReturnsOk()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("alpha");

        // First create a product to delete
        var newProduct = new CreateProductRequest
        {
            Name = "Product to Delete " + Guid.NewGuid(),
            Price = 50.00m
        };
        var createResponse = await client.PostAsJsonAsync("/api/Products", newProduct);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);

        // Act
        var response = await client.DeleteAsync($"/api/Products/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify deleted (soft delete - should not be found)
        var getResponse = await client.GetAsync($"/api/Products/{created.Id}/false");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    #endregion

    #region Complete HTTP CRUD Loop

    [Fact]
    public async Task FullHttpCrudLoop_CreateReadDelete_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("alpha");

        // Step 1: Create
        var createRequest = new CreateProductRequest
        {
            Name = "CRUD Loop Product " + Guid.NewGuid(),
            Price = 99.99m,
            Description = "Full CRUD test"
        };
        var createResponse = await client.PostAsJsonAsync("/api/Products", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        Assert.NotEqual(default, created.Id);

        // Step 2: Read
        var readResponse = await client.GetAsync($"/api/Products/{created.Id}/true");
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        var read = await readResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(read);
        Assert.Contains("CRUD Loop Product", read.Name);

        // Step 3: Delete
        var deleteResponse = await client.DeleteAsync($"/api/Products/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Verify deletion
        var verifyDeleteResponse = await client.GetAsync($"/api/Products/{created.Id}/false");
        Assert.Equal(HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task Products_GetPaged_ReturnsPaginatedResults()
    {
        // Arrange
        var client = _fixture.CreateClientWithTenant("alpha");

        // Act
        var response = await client.GetAsync("/api/Products/getpaged/1/2/false");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Response Models

    private class TenantInfoResponse
    {
        public long Id { get; set; }
        public string? Identifier { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
    }

    private class ProductResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

    private class CreateProductRequest
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

    #endregion
}
