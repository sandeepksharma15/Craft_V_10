using Moq;
using System.Net;
using Microsoft.Extensions.Logging;
using Craft.HttpServices.Services;
using Moq.Protected;

namespace Craft.HtppServices.Tests;

public class HttpChangeServiceTests
{
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

    [Fact]
    public async Task AddAsync_ReturnsSuccessResult_WhenResponseIsSuccess()
    {
        // Arrange
        var expected = new DummyEntity { Id = 42 };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.AddAsync(new DummyView { Id = 42 });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(42, result.Data.Id);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task AddAsync_ReturnsErrorResult_WhenResponseIsFailure()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Validation error\"]", System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.AddAsync(new DummyView { Id = 42 });

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Validation error", result.Errors);
    }

    [Fact]
    public async Task AddRangeAsync_ReturnsSuccessResult_WhenResponseIsSuccess()
    {
        // Arrange
        var expected = new List<DummyEntity> { new DummyEntity { Id = 1 }, new DummyEntity { Id = 2 } };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.AddRangeAsync(new List<DummyView> { new DummyView { Id = 1 }, new DummyView { Id = 2 } });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(1, result.Data[0].Id);
        Assert.Equal(2, result.Data[1].Id);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task AddRangeAsync_ReturnsErrorResult_WhenResponseIsFailure()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Batch error\"]", System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.AddRangeAsync(new List<DummyView> { new DummyView { Id = 1 } });

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Batch error", result.Errors);
    }

    public class DummyEntity : Craft.Domain.IEntity<int>, Craft.Domain.IModel<int>
    {
        public int Id { get; set; }
    }

    public class DummyView : Craft.Domain.IModel<int>
    {
        public int Id { get; set; }
    }

    public class DummyDto : Craft.Domain.IModel<int>
    {
        public int Id { get; set; }
    }
}
