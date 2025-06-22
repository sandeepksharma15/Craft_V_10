﻿using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Craft.Core.Tests.Common;

public class ServerResponseTests
{
    [Fact]
    public void Default_Constructor_Initializes_Properties()
    {
        // Arrange & Act
        var response = new ServerResponse();

        // Assert
        Assert.Null(response.Data);
        Assert.Null(response.ErrorId);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
        Assert.Null(response.ExpiryDate);
        Assert.False(response.IsSuccess);
        Assert.Null(response.Message);
        Assert.Null(response.Source);
        Assert.Equal(0, response.StatusCode);
        Assert.Null(response.SupportMessage);
    }

    [Fact]
    public void Can_Set_And_Get_All_Properties()
    {
        // Arrange & Act
        var now = DateTime.UtcNow;
        var errors = new List<string> { "error1", "error2" };
        var response = new ServerResponse
        {
            Data = 123,
            ErrorId = "E001",
            Errors = errors,
            ExpiryDate = now,
            IsSuccess = true,
            Message = "Success",
            Source = "UnitTest",
            StatusCode = 200,
            SupportMessage = "Contact support"
        };

        // Assert
        Assert.Equal(123, response.Data);
        Assert.Equal("E001", response.ErrorId);
        Assert.Equal(errors, response.Errors);
        Assert.Equal(now, response.ExpiryDate);
        Assert.True(response.IsSuccess);
        Assert.Equal("Success", response.Message);
        Assert.Equal("UnitTest", response.Source);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Contact support", response.SupportMessage);
    }

    [Fact]
    public void IdentityResult_Constructor_Sets_IsSuccess_And_Errors()
    {
        // Arrange
        var identityErrors = new List<IdentityError>
        {
            new() { Description = "desc1" },
            new() { Description = "desc2" }
        };

        // Act
        var identityResult = IdentityResult.Failed([.. identityErrors]);
        var response = new ServerResponse(identityResult);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(identityErrors.Select(e => e.Description), response.Errors);
        Assert.Null(response.Data);
    }

    [Fact]
    public void IdentityResult_Constructor_Sets_IsSuccess_True_When_Succeeded()
    {
        // Arrange & Act
        var identityResult = IdentityResult.Success;
        var response = new ServerResponse(identityResult);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Empty(response.Errors);
        Assert.Null(response.Data);
    }

    [Fact]
    public void ToString_Serializes_Object_To_Json()
    {
        // Arrange & Act
        var response = new ServerResponse
        {
            Data = new { Value = 1 },
            ErrorId = "E001",
            Errors = ["error1"],
            ExpiryDate = new DateTime(2024, 1, 1),
            IsSuccess = true,
            Message = "msg",
            Source = "src",
            StatusCode = 404,
            SupportMessage = "support"
        };
        var json = response.ToString();
        var deserialized = JsonSerializer.Deserialize<ServerResponse>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(response.ErrorId, deserialized.ErrorId);
        Assert.Equal(response.Errors, deserialized.Errors);
        Assert.Equal(response.ExpiryDate, deserialized.ExpiryDate);
        Assert.Equal(response.IsSuccess, deserialized.IsSuccess);
        Assert.Equal(response.Message, deserialized.Message);
        Assert.Equal(response.Source, deserialized.Source);
        Assert.Equal(response.StatusCode, deserialized.StatusCode);
        Assert.Equal(response.SupportMessage, deserialized.SupportMessage);
    }

    [Fact]
    public void Errors_Property_Can_Be_Set_To_Empty_Or_Null()
    {
        // Arrange & Act
        var response = new ServerResponse { Errors = [] };

        // Assert
        Assert.Empty(response.Errors);
        response.Errors = null!;
        Assert.Null(response.Errors);
    }
}
