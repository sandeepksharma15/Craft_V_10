using System.Net;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Craft.Security.Tests.Handlers;

public class BearerTokenHandlerTests
{
    [Fact]
    public async Task SendAsync_SetsAuthorizationHeader_WhenBearerTokenCookiePresent()
    {
        // Arrange
        var token = "test-token";
        var cookies = new Mock<IRequestCookieCollection>();
        cookies.Setup(c => c["BearerToken"]).Returns(token);
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(r => r.Cookies).Returns(cookies.Object);
        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(c => c.Request).Returns(requestMock.Object);
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);
        var handler = new BearerTokenHandler(accessorMock.Object)
        {
            InnerHandler = new DummyHandler()
        };
        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
        Assert.Equal(token, request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_DoesNotSetAuthorizationHeader_WhenBearerTokenCookieMissing()
    {
        // Arrange
        var cookies = new Mock<IRequestCookieCollection>();
        cookies.Setup(c => c["BearerToken"]).Returns((string?)null);
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(r => r.Cookies).Returns(cookies.Object);
        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(c => c.Request).Returns(requestMock.Object);
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);
        var handler = new BearerTokenHandler(accessorMock.Object)
        {
            InnerHandler = new DummyHandler()
        };
        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.Null(request.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_DoesNotSetAuthorizationHeader_WhenBearerTokenCookieEmpty()
    {
        // Arrange
        var cookies = new Mock<IRequestCookieCollection>();
        cookies.Setup(c => c["BearerToken"]).Returns(string.Empty);
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(r => r.Cookies).Returns(cookies.Object);
        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(c => c.Request).Returns(requestMock.Object);
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);
        var handler = new BearerTokenHandler(accessorMock.Object)
        {
            InnerHandler = new DummyHandler()
        };
        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.Null(request.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_DoesNotThrow_WhenHttpContextAccessorOrContextIsNull()
    {
        // IHttpContextAccessor is null
        var handler1 = new BearerTokenHandler(null!)
        {
            InnerHandler = new DummyHandler()
        };
        var client1 = new HttpClient(handler1);
        var request1 = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
        var ex1 = await Record.ExceptionAsync(() => client1.SendAsync(request1));
        Assert.Null(ex1);

        // HttpContext is null
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        var handler2 = new BearerTokenHandler(accessorMock.Object)
        {
            InnerHandler = new DummyHandler()
        };
        var client2 = new HttpClient(handler2);
        var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
        var ex2 = await Record.ExceptionAsync(() => client2.SendAsync(request2));
        Assert.Null(ex2);
    }

    private class DummyHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
