using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Craft.Core;
using Craft.HttpServices.Services;
using Craft.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Craft.Core.Common;

namespace Craft.HtppServices.Tests;

public class HttpReadServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsEntities_WhenResponseIsValid()
    {
        // Arrange
        var entities = new List<TestEntity> { new() { Id = 1 }, new() { Id = 2 } };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(entities)
        };
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains(result.Data, e => e.Id == 1);
        Assert.Contains(result.Data, e => e.Id == 2);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenResponseIsNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsError_WhenHttpFails()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task GetAsync_ReturnsEntity_WhenResponseIsValid()
    {
        // Arrange
        var entity = new TestEntity { Id = 42 };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(entity)
        };
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetAsync(42);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(42, result.Data.Id);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenResponseIsNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetAsync(99);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetAsync_ReturnsError_WhenHttpFails()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCount_WhenResponseIsValid()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(123L)
        };
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetCountAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(123L, result.Data);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsError_WhenHttpFails()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetCountAsync();

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsPageResponse_WhenResponseIsValid()
    {
        // Arrange
        var page = new PageResponse<TestEntity>([new TestEntity { Id = 1 }], 10, 1, 1);
        var json = JsonSerializer.Serialize(page);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetPagedListAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal(1, result.Data.Items.First().Id);
        Assert.Equal(10, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsError_WhenPageOrPageSizeInvalid()
    {
        // Arrange
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var ex1 = await Record.ExceptionAsync(async () => await service.GetPagedListAsync(0, 1));
        var ex2 = await Record.ExceptionAsync(async () => await service.GetPagedListAsync(1, 0));

        // Assert
        Assert.NotNull(ex1);
        Assert.NotNull(ex2);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsError_WhenHttpFails()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetPagedListAsync(1, 1);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsError_WhenDeserializationFails()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = CreateLogger();
        var service = new HttpReadService<TestEntity, int>(ApiUrl, httpClient, logger);

        // Act
        var result = await service.GetPagedListAsync(1, 1);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    // Helper for TResult tests
    public class AltResult { public string? Value { get; set; } }

    // Private test entity implementing required interfaces
    public class TestEntity : IEntity<int>, IModel<int>
    {
        public int Id { get; set; }
    }

    private static HttpClient CreateHttpClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();
        return new HttpClient(handlerMock.Object);
    }

    private static ILogger<HttpReadService<TestEntity, int>> CreateLogger(bool debugEnabled = false)
    {
        var loggerMock = new Mock<ILogger<HttpReadService<TestEntity, int>>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(debugEnabled);
        return loggerMock.Object;
    }

    private static Uri ApiUrl => new("http://localhost/api/test");
}
