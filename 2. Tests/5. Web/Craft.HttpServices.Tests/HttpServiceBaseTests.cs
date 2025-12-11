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
        Func<CancellationToken, Task<HttpServiceResult<PageResponse<TestItem>>?>> getPaged = _ => Task.FromResult<HttpServiceResult<PageResponse<TestItem>>?>(null);
        Func<PageResponse<TestItem>, List<TestItem>> extractItems = p => p.Items.ToList();

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_ReturnsEmptyList_WhenDataIsNull()
    {
        // Arrange
        var pagedResult = new HttpServiceResult<PageResponse<TestItem>>
        {
            Success = true,
            Data = null
        };
        Func<CancellationToken, Task<HttpServiceResult<PageResponse<TestItem>>?>> getPaged = _ => Task.FromResult<HttpServiceResult<PageResponse<TestItem>>?>(pagedResult);
        Func<PageResponse<TestItem>, List<TestItem>> extractItems = p => p.Items.ToList();

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
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
            Success = true,
            Data = pageResponse,
            StatusCode = 200
        };
        Func<CancellationToken, Task<HttpServiceResult<PageResponse<TestItem>>?>> getPaged = _ => Task.FromResult<HttpServiceResult<PageResponse<TestItem>>?>(pagedResult);
        Func<PageResponse<TestItem>, List<TestItem>> extractItems = p => p.Items.ToList();

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
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
            Success = false,
            Errors = ["API Error"],
            StatusCode = 500
        };
        Func<CancellationToken, Task<HttpServiceResult<PageResponse<TestItem>>?>> getPaged = _ => Task.FromResult<HttpServiceResult<PageResponse<TestItem>>?>(pagedResult);
        Func<PageResponse<TestItem>, List<TestItem>> extractItems = p => p.Items.ToList();

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("API Error", result.Errors);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_HandlesException_WhenGetPagedThrows()
    {
        // Arrange
        Func<CancellationToken, Task<HttpServiceResult<PageResponse<TestItem>>?>> getPaged = _ => throw new InvalidOperationException("Test exception");
        Func<PageResponse<TestItem>, List<TestItem>> extractItems = p => p.Items.ToList();

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("Test exception", result.Errors);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_PropagatesOperationCanceledException()
    {
        // Arrange
        Func<CancellationToken, Task<HttpServiceResult<PageResponse<TestItem>>?>> getPaged = _ => throw new OperationCanceledException();
        Func<PageResponse<TestItem>, List<TestItem>> extractItems = p => p.Items.ToList();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None));
    }

    public class TestItem
    {
        public int Id { get; set; }
    }

    public class TestHttpServiceBase : HttpServiceBase
    {
        public TestHttpServiceBase(Uri apiURL, HttpClient httpClient) : base(apiURL, httpClient)
        {
        }

        public Task<HttpServiceResult<TResult?>> TestGetAndParseAsync<TResult>(
            Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
            Func<HttpContent, CancellationToken, Task<TResult?>> parser,
            CancellationToken cancellationToken)
        {
            return GetAndParseAsync(sendRequest, parser, cancellationToken);
        }

        public Task<HttpServiceResult<TResult?>> TestSendAndParseAsync<TResult>(
            Func<Task<HttpResponseMessage>> sendRequest,
            Func<HttpContent, CancellationToken, Task<TResult?>> parser,
            CancellationToken cancellationToken)
        {
            return SendAndParseAsync(sendRequest, parser, cancellationToken);
        }

        public Task<HttpServiceResult<bool>> TestSendAndParseNoContentAsync(
            Func<Task<HttpResponseMessage>> sendRequest,
            CancellationToken cancellationToken)
        {
            return SendAndParseNoContentAsync(sendRequest, cancellationToken);
        }
    }

    [Fact]
    public async Task SendAndParseAsync_ReturnsError_WhenSendRequestIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new TestHttpServiceBase(new Uri("http://localhost"), httpClient);
        Func<HttpContent, CancellationToken, Task<string?>> parser = (c, ct) => Task.FromResult<string?>("test");

        // Act
        var result = await service.TestSendAndParseAsync<string>(null!, parser, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("sendRequest delegate is null.", result.Errors);
    }

    [Fact]
    public async Task SendAndParseAsync_ReturnsError_WhenParserIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new TestHttpServiceBase(new Uri("http://localhost"), httpClient);
        Func<Task<HttpResponseMessage>> sendRequest = () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var result = await service.TestSendAndParseAsync<string>(sendRequest, null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("parser delegate is null.", result.Errors);
    }

    [Fact]
    public async Task SendAndParseAsync_HandlesException_FromSendRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new TestHttpServiceBase(new Uri("http://localhost"), httpClient);
        Func<Task<HttpResponseMessage>> sendRequest = () => throw new InvalidOperationException("Send error");
        Func<HttpContent, CancellationToken, Task<string?>> parser = (c, ct) => Task.FromResult<string?>("test");

        // Act
        var result = await service.TestSendAndParseAsync(sendRequest, parser, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
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
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest = ct => httpClient.GetAsync("http://localhost", ct);
        Func<HttpContent, CancellationToken, Task<string?>> parser = async (c, ct) => await c.ReadAsStringAsync(ct);

        // Act
        var result = await service.TestGetAndParseAsync(sendRequest, parser, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
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
        Func<Task<HttpResponseMessage>> sendRequest = () => Task.FromResult(response);

        // Act
        var result = await service.TestSendAndParseNoContentAsync(sendRequest, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
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
        Func<Task<HttpResponseMessage>> sendRequest = () => Task.FromResult(response);

        // Act
        var result = await service.TestSendAndParseNoContentAsync(sendRequest, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.False(result.Data);
        Assert.Equal(400, result.StatusCode);
    }
}
