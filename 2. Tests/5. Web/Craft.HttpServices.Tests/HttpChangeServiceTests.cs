using System.Net;
using Craft.HttpServices.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Craft.HtppServices.Tests;

public class HttpChangeServiceTests
{
    [Fact]
    public async Task AddAsync_ThrowsArgumentNullException_WhenModelIsNull()
    {
        // Arrange
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAsync(null!));
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
    public async Task AddAsync_ReturnsErrorResult_WhenErrorResponseIsNotJson()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Some plain error", System.Text.Encoding.UTF8, "text/plain")
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
        Assert.Contains("Some plain error", result.Errors);
    }

    [Fact]
    public async Task AddAsync_RespectsCancellationToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.AddAsync(new DummyView { Id = 1 }, cts.Token));
    }

    [Fact]
    public async Task AddAsync_LogsDebug_WhenLoggerEnabled()
    {
        // Arrange
        var expected = new DummyEntity { Id = 42 };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var loggerMock = new Mock<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, loggerMock.Object);

        // Act
        await service.AddAsync(new DummyView { Id = 42 });

        // Assert
        loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AddAsync")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task AddRangeAsync_ThrowsArgumentNullException_WhenModelsIsNull()
    {
        // Arrange
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddRangeAsync(null!));
    }

    [Fact]
    public async Task AddRangeAsync_ReturnsSuccessResult_WhenResponseIsSuccess()
    {
        // Arrange
        var expected = new List<DummyEntity> { new() { Id = 1 }, new() { Id = 2 } };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.AddRangeAsync([new DummyView { Id = 1 }, new DummyView { Id = 2 }]);

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
        var result = await service.AddRangeAsync([new DummyView { Id = 1 }]);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Batch error", result.Errors);
    }

    [Fact]
    public async Task AddRangeAsync_ReturnsErrorResult_WhenErrorResponseIsNotJson()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Some batch error", System.Text.Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.AddRangeAsync([new DummyView { Id = 1 }]);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Some batch error", result.Errors);
    }

    [Fact]
    public async Task AddRangeAsync_EmptyList_ReturnsSuccessWithEmptyData()
    {
        // Arrange
        var expected = new List<DummyEntity>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.AddRangeAsync([]);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task AddRangeAsync_RespectsCancellationToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.AddRangeAsync([new DummyView { Id = 1 }], cts.Token));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsSuccessResult_WhenResponseIsSuccess()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsErrorResult_WhenResponseIsFailure()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Delete error\"]", System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.False(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Delete error", result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsErrorResult_WhenErrorResponseIsNotJson()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Delete plain error", System.Text.Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.False(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Delete plain error", result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_RespectsCancellationToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.DeleteAsync(1, cts.Token));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsArgumentNullException_WhenUsingReferenceTypeKey()
    {
        // Arrange
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntityWithStringKey, DummyViewWithStringKey, DummyDtoWithStringKey, string>>>();
        var service = new HttpChangeService<DummyEntityWithStringKey, DummyViewWithStringKey, DummyDtoWithStringKey, string>(new Uri("http://localhost/api"), httpClient, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.DeleteAsync(null!));
    }

    [Fact]
    public async Task DeleteRangeAsync_ThrowsArgumentNullException_WhenModelsIsNull()
    {
        // Arrange
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.DeleteRangeAsync(null!));
    }

    [Fact]
    public async Task DeleteRangeAsync_ReturnsSuccessResult_WhenResponseIsSuccess()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.DeleteRangeAsync([new DummyView { Id = 1 }]);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task DeleteRangeAsync_ReturnsErrorResult_WhenResponseIsFailure()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Delete range error\"]", System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.DeleteRangeAsync([new DummyView { Id = 1 }]);

        // Assert
        Assert.False(result.Success);
        Assert.False(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Delete range error", result.Errors);
    }

    [Fact]
    public async Task DeleteRangeAsync_ReturnsErrorResult_WhenErrorResponseIsNotJson()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Delete range plain error", System.Text.Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.DeleteRangeAsync([new DummyView { Id = 1 }]);

        // Assert
        Assert.False(result.Success);
        Assert.False(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Delete range plain error", result.Errors);
    }

    [Fact]
    public async Task DeleteRangeAsync_RespectsCancellationToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.DeleteRangeAsync([new DummyView { Id = 1 }], cts.Token));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentNullException_WhenModelIsNull()
    {
        // Arrange
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsSuccessResult_WhenResponseIsSuccess()
    {
        // Arrange
        var expected = new DummyEntity { Id = 99 };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.UpdateAsync(new DummyView { Id = 99 });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(99, result.Data.Id);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsErrorResult_WhenResponseIsFailure()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Update error\"]", System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.UpdateAsync(new DummyView { Id = 99 });

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Update error", result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsErrorResult_WhenErrorResponseIsNotJson()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Update plain error", System.Text.Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.UpdateAsync(new DummyView { Id = 99 });

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Update plain error", result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_RespectsCancellationToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.UpdateAsync(new DummyView { Id = 99 }, cts.Token));
    }

    [Fact]
    public async Task UpdateRangeAsync_ThrowsArgumentNullException_WhenModelsIsNull()
    {
        // Arrange
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateRangeAsync(null!));
    }

    [Fact]
    public async Task UpdateRangeAsync_ReturnsSuccessResult_WhenResponseIsSuccess()
    {
        // Arrange
        var expected = new List<DummyEntity> { new() { Id = 5 }, new() { Id = 6 } };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.UpdateRangeAsync([new DummyView { Id = 5 }, new DummyView { Id = 6 }]);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(5, result.Data[0].Id);
        Assert.Equal(6, result.Data[1].Id);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task UpdateRangeAsync_ReturnsErrorResult_WhenResponseIsFailure()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Update range error\"]", System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.UpdateRangeAsync([new DummyView { Id = 5 }]);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Update range error", result.Errors);
    }

    [Fact]
    public async Task UpdateRangeAsync_ReturnsErrorResult_WhenErrorResponseIsNotJson()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Update range plain error", System.Text.Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.UpdateRangeAsync([new() { Id = 5 }]);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Update range plain error", result.Errors);
    }

    [Fact]
    public async Task UpdateRangeAsync_EmptyList_ReturnsSuccessWithEmptyData()
    {
        // Arrange
        var expected = new List<DummyEntity>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.UpdateRangeAsync([]);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task UpdateRangeAsync_RespectsCancellationToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.UpdateRangeAsync([new DummyView { Id = 5 }], cts.Token));
    }

    [Fact]
    public async Task UpdateRangeAsync_HandlesNetworkError()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network failure"));
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpChangeService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpChangeService<DummyEntity, DummyView, DummyDto, int>(new Uri("http://localhost/api"), httpClient, logger);

        // Act
        var result = await service.UpdateRangeAsync([new DummyView { Id = 5 }]);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("Network failure", result.Errors);
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

    public class DummyEntityWithStringKey : Craft.Domain.IEntity<string>, Craft.Domain.IModel<string>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class DummyViewWithStringKey : Craft.Domain.IModel<string>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class DummyDtoWithStringKey : Craft.Domain.IModel<string>
    {
        public string Id { get; set; } = string.Empty;
    }
}
