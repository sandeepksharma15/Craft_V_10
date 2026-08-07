using Craft.Emails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Tests.Emails;

public class EmailControllerTests
{
    [Fact]
    public async Task SendAsync_WhenEmailIsSent_ReturnsOk()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test Subject",
            body: "Test Body");
        var expectedResult = EmailResult.Success("message-id");

        var mailServiceMock = new Mock<IMailService>();
        mailServiceMock
            .Setup(service => service.SendAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var loggerMock = new Mock<ILogger<EmailControllerBase>>();
        var controller = new EmailController(mailServiceMock.Object, loggerMock.Object);

        // Act
        var actionResult = await controller.SendAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var emailResult = Assert.IsType<EmailResult>(okResult.Value);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedResult, emailResult);
    }

    [Fact]
    public async Task SendAsync_WhenEmailFails_ReturnsInternalServerError()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test Subject",
            body: "Test Body");
        var expectedResult = EmailResult.Failure("SMTP unavailable");

        var mailServiceMock = new Mock<IMailService>();
        mailServiceMock
            .Setup(service => service.SendAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var loggerMock = new Mock<ILogger<EmailControllerBase>>();
        var controller = new EmailController(mailServiceMock.Object, loggerMock.Object);

        // Act
        var actionResult = await controller.SendAsync(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var emailResult = Assert.IsType<EmailResult>(objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Same(expectedResult, emailResult);
    }
}
