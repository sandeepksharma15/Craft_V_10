using System.Net;
using Craft.Core;
using Craft.Core.Common;
using Craft.HttpServices;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Craft.HtppServices.Tests;

public class HttpServiceBaseTests
{
    [Fact]
    public async Task GetAllFromPagedAsync_ReturnsEmptyList_WhenPagedResultIsNull()
    {
        // Arrange
        Task<HttpServiceResult<PageResponse<TestItem>>?> GetPaged(CancellationToken _) => Task.FromResult<HttpServiceResult<PageResponse<TestItem>>?>(null);
        List<TestItem> ExtractItems(PageResponse<TestItem> p) => [.. p.Items];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(GetPaged, ExtractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_ReturnsEmptyList_WhenDataIsNull()
    {
        // Arrange
        var pagedResult = new HttpServiceResult<PageResponse<TestItem>>
        {
            IsSuccess = true,
            Data = null
        };
        Task<HttpServiceResult<PageResponse<TestItem>>?> GetPaged(CancellationToken _) => Task.FromResult<HttpServiceResult<PageResponse<TestItem>>?>(pagedResult);
        List<TestItem> ExtractItems(PageResponse<TestItem> p) => [.. p.Items];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(GetPaged, ExtractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_ReturnsItems_WhenDataIsValid()
    {
        // Arrange
        var items = new List<TestItem> { new() { Id = 1 }, new() { Id = 2 } };
        var pageResponse = new PageResponse<TestItem>(items, 2, 1, 1);
        var pagedResult = new HttpServiceResult<PageResponse<TestItem>>
        {
            IsSuccess = true,
            Data = pageResponse,
            StatusCode = 200
        };
        Task<HttpServiceResult<PageResponse<TestItem>>?> GetPaged(CancellationToken _) => Task.FromResult<HttpServiceResult<PageResponse<TestItem>>?>(pagedResult);
        List<TestItem> ExtractItems(PageResponse<TestItem> p) => [.. p.Items];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(GetPaged, ExtractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_PropagatesErrors_WhenPagedResultHasErrors()
    {
        // Arrange
        var pagedResult = new HttpServiceResult<PageResponse<TestItem>>
        {
            IsSuccess = false,
            Errors = ["API Error"],
            StatusCode = 500
        };
        Task<HttpServiceResult<PageResponse<TestItem>>?> GetPaged(CancellationToken _) => Task.FromResult<HttpServiceResult<PageResponse<TestItem>>?>(pagedResult);
        List<TestItem> ExtractItems(PageResponse<TestItem> p) => [.. p.Items];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(GetPaged, ExtractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Contains("API Error", result.Errors);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_HandlesException_WhenGetPagedThrows()
    {
        // Arrange
        Task<HttpServiceResult<PageResponse<TestItem>>?> GetPaged(CancellationToken _) => throw new InvalidOperationException("Test exception");
        List<TestItem> ExtractItems(PageResponse<TestItem> p) => [.. p.Items];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(GetPaged, ExtractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Contains("Test exception", result.Errors);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_PropagatesOperationCanceledException()
    {
        // Arrange
        Task<HttpServiceResult<PageResponse<TestItem>>?> GetPaged(CancellationToken _) => throw new OperationCanceledException();
        List<TestItem> ExtractItems(PageResponse<TestItem> p) => [.. p.Items];

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            HttpServiceBase.GetAllFromPagedAsync(GetPaged, ExtractItems, CancellationToken.None));
    }

    public class TestItem
    {
        public int Id { get; set; }
    }

    public class TestHttpServiceBase(Uri apiURL, HttpClient httpClient) : HttpServiceBase(apiURL, httpClient)
    {
#pragma warning disable CA1822 // Mark members as static
        public Task<HttpServiceResult<TResult?>> TestGetAndParseAsync<TResult>(Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
            Func<HttpContent, CancellationToken, Task<TResult?>> parser, CancellationToken cancellationToken) =>
            GetAndParseAsync(sendRequest, parser, cancellationToken);

        public Task<HttpServiceResult<TResult?>> TestSendAndParseAsync<TResult>(Func<Task<HttpResponseMessage>> sendRequest,
            Func<HttpContent, CancellationToken, Task<TResult?>> parser, CancellationToken cancellationToken) =>
            SendAndParseAsync(sendRequest, parser, cancellationToken);

        public Task<HttpServiceResult<bool>> TestSendAndParseNoContentAsync(Func<Task<HttpResponseMessage>> sendRequest, CancellationToken cancellationToken) 
            => SendAndParseNoContentAsync(sendRequest, cancellationToken);
#pragma warning restore CA1822 // Mark members as static
    }

    [Fact]
    public async Task SendAndParseAsync_ReturnsError_WhenSendRequestIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new TestHttpServiceBase(new Uri("http://localhost"), httpClient);
        static Task<string?> Parser(HttpContent c, CancellationToken ct) => Task.FromResult<string?>("test");

        // Act
        var result = await service.TestSendAndParseAsync<string>(null!, Parser, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Contains("sendRequest delegate is null.", result.Errors);
    }

    [Fact]
    public async Task SendAndParseAsync_ReturnsError_WhenParserIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new TestHttpServiceBase(new Uri("http://localhost"), httpClient);
        static Task<HttpResponseMessage> SendRequest() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var result = await service.TestSendAndParseAsync<string>(SendRequest, null!, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Contains("parser delegate is null.", result.Errors);
    }

    [Fact]
    public async Task SendAndParseAsync_HandlesException_FromSendRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new TestHttpServiceBase(new Uri("http://localhost"), httpClient);
        Task<HttpResponseMessage> SendRequest() => throw new InvalidOperationException("Send error");
        Task<string?> Parser(HttpContent c, CancellationToken ct) => Task.FromResult<string?>("test");

        // Act
        var result = await service.TestSendAndParseAsync(SendRequest, Parser, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Send error", result.Errors[0]);
    }

    [Fact]
    public async Task GetAndParseAsync_ReturnsSuccess_WhenResponseIsValid()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("\"test data\"", System.Text.Encoding.UTF8, "application/json")
        };
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new TestHttpServiceBase(new Uri("http://localhost"), httpClient);
        Task<HttpResponseMessage> SendRequest(CancellationToken ct) => httpClient.GetAsync(new Uri("http://localhost"), ct);
        async Task<string?> Parser(HttpContent c, CancellationToken ct) => await c.ReadAsStringAsync(ct);

        // Act
        var result = await service.TestGetAndParseAsync(SendRequest, Parser, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task SendAndParseNoContentAsync_ReturnsTrue_OnSuccess()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new TestHttpServiceBase(new Uri("http://localhost"), httpClient);
        Task<HttpResponseMessage> SendRequest() => Task.FromResult(response);

        // Act
        var result = await service.TestSendAndParseNoContentAsync(SendRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task SendAndParseNoContentAsync_ReturnsFalse_OnFailure()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("[\"Error\"]", System.Text.Encoding.UTF8, "application/json")
        };
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new TestHttpServiceBase(new Uri("http://localhost"), httpClient);
        Task<HttpResponseMessage> SendRequest() => Task.FromResult(response);

        // Act
        var result = await service.TestSendAndParseNoContentAsync(SendRequest, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.False(result.Data);
        Assert.Equal(400, result.StatusCode);
    }
}
