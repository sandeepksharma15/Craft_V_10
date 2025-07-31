using Moq;
using Moq.Protected;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Craft.Core;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Craft.QuerySpec.Tests.Services;

public class HttpServiceTests
{
    private static readonly Uri ApiUrl = new("http://localhost/api");

    // --- DeleteAsync ---
    [Fact]
    public async Task DeleteAsync_ReturnsSuccess_WhenResponseIsSuccess()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.DeleteAsync(query);

        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsError_WhenResponseIsFailure()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Delete error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.DeleteAsync(query);

        Assert.False(result.Success);
        Assert.False(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Delete error", result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsError_WhenResponseIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Delete plain error", Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.DeleteAsync(query);

        Assert.False(result.Success);
        Assert.False(result.Data);
        Assert.NotNull(result.Errors);
        Assert.Contains("Delete plain error", result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.DeleteAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_PropagatesOperationCanceledException_WhenCancelled()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.DeleteAsync(query, cts.Token));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsError_WhenResponseIsFailure()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"GetAll error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetAllAsync(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetAll error", result.Errors);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsError_WhenResponseIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("GetAll plain error", Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetAllAsync(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetAll plain error", result.Errors);
    }

    [Fact]
    public async Task GetAllAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetAllAsync(null!));
    }

    [Fact]
    public async Task GetAllAsync_PropagatesOperationCanceledException_WhenCancelled()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetAllAsync(query, cts.Token));
    }

    [Fact]
    public async Task GetAllAsync_Projected_ReturnsError_WhenResponseIsFailure()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"GetAll projected error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();

        var result = await service.GetAllAsync<DummyDto>(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetAll projected error", result.Errors);
    }

    [Fact]
    public async Task GetAllAsync_Projected_ReturnsError_WhenResponseIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("GetAll projected plain error", Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();

        var result = await service.GetAllAsync<DummyDto>(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetAll projected plain error", result.Errors);
    }

    [Fact]
    public async Task GetAllAsync_Projected_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetAllAsync<DummyDto>(null!));
    }

    [Fact]
    public async Task GetAllAsync_Projected_PropagatesOperationCanceledException_WhenCancelled()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetAllAsync<DummyDto>(query, cts.Token));
    }

    // --- GetAsync (entity) ---
    [Fact]
    public async Task GetAsync_ReturnsEntity_WhenResponseIsSuccess()
    {
        var entity = new DummyEntity { Id = 42 };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetAsync(query);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(42, result.Data.Id);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenEntityNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetAsync(query);

        Assert.True(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetAsync_ReturnsError_WhenResponseIsFailure()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"GetAsync error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetAsync(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetAsync error", result.Errors);
    }

    [Fact]
    public async Task GetAsync_ReturnsError_WhenResponseIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("GetAsync plain error", Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetAsync(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetAsync plain error", result.Errors);
    }

    [Fact]
    public async Task GetAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetAsync(null!));
    }

    [Fact]
    public async Task GetAsync_PropagatesOperationCanceledException_WhenCancelled()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetAsync(query, cts.Token));
    }

    // --- GetAsync (projected) ---
    [Fact]
    public async Task GetAsync_Projected_ReturnsEntity_WhenResponseIsSuccess()
    {
        var entity = new DummyDto { Id = 42 };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();

        var result = await service.GetAsync<DummyDto>(query);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(42, result.Data.Id);
    }

    [Fact]
    public async Task GetAsync_Projected_ReturnsNull_WhenEntityNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();

        var result = await service.GetAsync<DummyDto>(query);

        Assert.True(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetAsync_Projected_ReturnsError_WhenResponseIsFailure()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"GetAsync projected error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();

        var result = await service.GetAsync<DummyDto>(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetAsync projected error", result.Errors);
    }

    [Fact]
    public async Task GetAsync_Projected_ReturnsError_WhenResponseIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("GetAsync projected plain error", Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();

        var result = await service.GetAsync<DummyDto>(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetAsync projected plain error", result.Errors);
    }

    [Fact]
    public async Task GetAsync_Projected_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetAsync<DummyDto>(null!));
    }

    [Fact]
    public async Task GetAsync_Projected_PropagatesOperationCanceledException_WhenCancelled()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetAsync<DummyDto>(query, cts.Token));
    }

    // --- GetCountAsync ---
    [Fact]
    public async Task GetCountAsync_ReturnsCount_WhenResponseIsSuccess()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(42L), Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetCountAsync(query);

        Assert.True(result.Success);
        Assert.Equal(42L, result.Data);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsError_WhenResponseIsFailure()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"GetCount error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetCountAsync(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetCount error", result.Errors);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsError_WhenResponseIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("GetCount plain error", Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetCountAsync(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetCount plain error", result.Errors);
    }

    [Fact]
    public async Task GetCountAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetCountAsync(null!));
    }

    [Fact]
    public async Task GetCountAsync_PropagatesOperationCanceledException_WhenCancelled()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetCountAsync(query, cts.Token));
    }

    // --- GetPagedListAsync (entity) ---
    [Fact]
    public async Task GetPagedListAsync_ReturnsPage_WhenResponseIsSuccess()
    {
        var entities = new List<DummyEntity> { new() { Id = 1 }, new() { Id = 2 } };
        var page = new PageResponse<DummyEntity>(entities, 2, 1, 2);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(page), Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetPagedListAsync(query);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Items.Count());
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsError_WhenResponseIsFailure()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"GetPagedList error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetPagedListAsync(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetPagedList error", result.Errors);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsError_WhenResponseIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("GetPagedList plain error", Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetPagedListAsync(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetPagedList plain error", result.Errors);
    }

    [Fact]
    public async Task GetPagedListAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetPagedListAsync(null!));
    }

    [Fact]
    public async Task GetPagedListAsync_PropagatesOperationCanceledException_WhenCancelled()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetPagedListAsync(query, cts.Token));
    }

    // --- GetPagedListAsync (projected) ---
    [Fact]
    public async Task GetPagedListAsync_Projected_ReturnsPage_WhenResponseIsSuccess()
    {
        var entities = new List<DummyDto> { new() { Id = 1 }, new() { Id = 2 } };
        var page = new PageResponse<DummyDto>(entities, 2, 1, 2);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(page), Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();

        var result = await service.GetPagedListAsync<DummyDto>(query);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Items.Count());
    }

    [Fact]
    public async Task GetPagedListAsync_Projected_ReturnsError_WhenResponseIsFailure()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"GetPagedList projected error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();

        var result = await service.GetPagedListAsync<DummyDto>(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetPagedList projected error", result.Errors);
    }

    [Fact]
    public async Task GetPagedListAsync_Projected_ReturnsError_WhenResponseIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("GetPagedList projected plain error", Encoding.UTF8, "text/plain")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();

        var result = await service.GetPagedListAsync<DummyDto>(query);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("GetPagedList projected plain error", result.Errors);
    }

    [Fact]
    public async Task GetPagedListAsync_Projected_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetPagedListAsync<DummyDto>(null!));
    }

    [Fact]
    public async Task GetPagedListAsync_Projected_PropagatesOperationCanceledException_WhenCancelled()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
        var httpClient = new HttpClient(handlerMock.Object);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity, DummyDto>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetPagedListAsync<DummyDto>(query, cts.Token));
    }

    /// <summary>
    /// Verifies that StatusCode is set for GetAllAsync error.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_SetsStatusCode_OnError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);
        var query = new DummyQuery<DummyEntity>();

        var result = await service.GetAllAsync(query);

        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    /// <summary>
    /// Verifies that debug logging occurs on error if enabled.
    /// </summary>
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "<Pending>")]
    public async Task All_Methods_LogDebug_OnError_WhenLoggerEnabled()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Error\"]", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var loggerMock = new Mock<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, loggerMock.Object);
        var query = new DummyQuery<DummyEntity>();

        var _ = await service.DeleteAsync(query);
        loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("DeleteAsync")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that null/empty server responses are handled for projected and paged methods.
    /// </summary>
    [Fact]
    public async Task All_Methods_Handle_NullOrEmptyServerResponses_Projected_And_Paged()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(response);
        var logger = Mock.Of<ILogger<HttpService<DummyEntity, DummyView, DummyDto, int>>>();
        var service = new HttpService<DummyEntity, DummyView, DummyDto, int>(ApiUrl, httpClient, logger);

        var getAllProjectedResult = await service.GetAllAsync<DummyDto>(new DummyQuery<DummyEntity, DummyDto>());
        Assert.True(getAllProjectedResult.Success);
        Assert.Null(getAllProjectedResult.Data);

        var getPagedListResult = await service.GetPagedListAsync(new DummyQuery<DummyEntity>());
        Assert.False(getPagedListResult.Success);
        Assert.Null(getPagedListResult.Data);
    }

    // --- Dummy types and helpers ---
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

    // DummyQuery for entity only
    public class DummyQuery<TEntity> : IQuery<TEntity> where TEntity : class
    {
        public bool AsNoTracking { get; set; }
        public bool AsSplitQuery { get; set; }
        public bool IgnoreAutoIncludes { get; set; }
        public bool IgnoreQueryFilters { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public SortOrderBuilder<TEntity>? SortOrderBuilder { get; set; }
        public Func<IEnumerable<TEntity>, IEnumerable<TEntity>>? PostProcessingAction { get; set; }
        public SqlLikeSearchCriteriaBuilder<TEntity>? SqlLikeSearchCriteriaBuilder { get; set; }
        public EntityFilterBuilder<TEntity>? EntityFilterBuilder { get; set; }
        public void Clear() { }
        public bool IsSatisfiedBy(TEntity entity) => true;
        public void SetPage(int page, int pageSize) { }
    }

    // DummyQuery for entity and projection
    public class DummyQuery<TEntity, TProjection> : IQuery<TEntity, TProjection> where TEntity : class where TProjection : class
    {
        public bool AsNoTracking { get; set; }
        public bool AsSplitQuery { get; set; }
        public bool IgnoreAutoIncludes { get; set; }
        public bool IgnoreQueryFilters { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public SortOrderBuilder<TEntity>? SortOrderBuilder { get; set; }
        public Func<IEnumerable<TEntity>, IEnumerable<TEntity>>? PostProcessingAction { get; set; }
        Func<IEnumerable<TProjection>, IEnumerable<TProjection>>? IQuery<TEntity, TProjection>.PostProcessingAction { get; set; }
        public SqlLikeSearchCriteriaBuilder<TEntity>? SqlLikeSearchCriteriaBuilder { get; set; }
        public EntityFilterBuilder<TEntity>? EntityFilterBuilder { get; set; }
        public QuerySelectBuilder<TEntity, TProjection>? QuerySelectBuilder { get; set; }
        public System.Linq.Expressions.Expression<Func<TEntity, IEnumerable<TProjection>>>? SelectorMany { get; set; }
        public void Clear() { }
        public bool IsSatisfiedBy(TEntity entity) => true;
        public void SetPage(int page, int pageSize) { }
    }
}
