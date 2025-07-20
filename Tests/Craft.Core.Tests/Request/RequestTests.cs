using System.Text.Json;
using Craft.Core.Request;

namespace Craft.Core.Tests.Request;

/// <summary>
/// Unit tests for APIRequest and its derived types, including serialization and deserialization.
/// </summary>
public class RequestTests
{
    [Fact]
    public void GetRequest_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        int id = 42;
        bool includeDetails = true;

        // Act
        var request = new GetRequest(id, includeDetails);

        // Assert
        Assert.Equal(ApiRequestType.Get, request.RequestType);
        Assert.Equal(id, request.Id);
        Assert.Equal(includeDetails, request.IncludeDetails);
    }

    [Fact]
    public void GetAllRequest_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        bool includeDetails = false;

        // Act
        var request = new GetAllRequest(includeDetails);

        // Assert
        Assert.Equal(ApiRequestType.GetAll, request.RequestType);
        Assert.Equal(includeDetails, request.IncludeDetails);
    }

    [Fact]
    public void GetPagedRequest_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        int currentPage = 3;
        int pageSize = 25;
        bool includeDetails = true;

        // Act
        var request = new GetPagedRequest(currentPage, pageSize, includeDetails);

        // Assert
        Assert.Equal(ApiRequestType.GetPaged, request.RequestType);
        Assert.Equal(currentPage, request.CurrentPage);
        Assert.Equal(pageSize, request.PageSize);
        Assert.Equal(includeDetails, request.IncludeDetails);
    }

    [Fact]
    public void GetRequest_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetRequest(7, true);

        // Act
        string json = JsonSerializer.Serialize<APIRequest>(original);
        var deserialized = JsonSerializer.Deserialize<APIRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<GetRequest>(deserialized);
        var getRequest = (GetRequest)deserialized;
        Assert.Equal(7, getRequest.Id);
        Assert.True(getRequest.IncludeDetails);
        Assert.Equal(ApiRequestType.Get, getRequest.RequestType);
    }

    [Fact]
    public void GetAllRequest_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetAllRequest(false);

        // Act
        string json = JsonSerializer.Serialize<APIRequest>(original);
        var deserialized = JsonSerializer.Deserialize<APIRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<GetAllRequest>(deserialized);
        var getAllRequest = (GetAllRequest)deserialized;
        Assert.False(getAllRequest.IncludeDetails);
        Assert.Equal(ApiRequestType.GetAll, getAllRequest.RequestType);
    }

    [Fact]
    public void GetPagedRequest_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetPagedRequest(2, 50, true);

        // Act
        string json = JsonSerializer.Serialize<APIRequest>(original);
        var deserialized = JsonSerializer.Deserialize<APIRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<GetPagedRequest>(deserialized);
        var getPagedRequest = (GetPagedRequest)deserialized;
        Assert.Equal(2, getPagedRequest.CurrentPage);
        Assert.Equal(50, getPagedRequest.PageSize);
        Assert.True(getPagedRequest.IncludeDetails);
        Assert.Equal(ApiRequestType.GetPaged, getPagedRequest.RequestType);
    }

    [Fact]
    public void GetRequest_Serialization_And_Deserialization_AsBaseType_Works()
    {
        // Arrange
        APIRequest original = new GetRequest(99, false);

        // Act
        string json = JsonSerializer.Serialize<APIRequest>(original);
        var deserialized = JsonSerializer.Deserialize<APIRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<GetRequest>(deserialized);
        var getRequest = (GetRequest)deserialized;
        Assert.Equal(99, getRequest.Id);
        Assert.False(getRequest.IncludeDetails);
        Assert.Equal(ApiRequestType.Get, getRequest.RequestType);
    }

    [Fact]
    public void GetAllRequest_Serialization_And_Deserialization_AsBaseType_Works()
    {
        // Arrange
        APIRequest original = new GetAllRequest(true);

        // Act
        string json = JsonSerializer.Serialize<APIRequest>(original);
        var deserialized = JsonSerializer.Deserialize<APIRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<GetAllRequest>(deserialized);
        var getAllRequest = (GetAllRequest)deserialized;
        Assert.True(getAllRequest.IncludeDetails);
        Assert.Equal(ApiRequestType.GetAll, getAllRequest.RequestType);
    }

    [Fact]
    public void GetPagedRequest_Serialization_And_Deserialization_AsBaseType_Works()
    {
        // Arrange
        APIRequest original = new GetPagedRequest(1, 10, false);

        // Act
        string json = JsonSerializer.Serialize<APIRequest>(original);
        var deserialized = JsonSerializer.Deserialize<APIRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<GetPagedRequest>(deserialized);
        var getPagedRequest = (GetPagedRequest)deserialized;
        Assert.Equal(1, getPagedRequest.CurrentPage);
        Assert.Equal(10, getPagedRequest.PageSize);
        Assert.False(getPagedRequest.IncludeDetails);
        Assert.Equal(ApiRequestType.GetPaged, getPagedRequest.RequestType);
    }

    [Fact]
    public void GetPagedRequest_ZeroAndNegativeValues_AreHandled()
    {
        // Arrange
        var zeroRequest = new GetPagedRequest(0, 0, false);
        var negativeRequest = new GetPagedRequest(-1, -10, true);

        // Act & Assert
        Assert.Equal(0, zeroRequest.CurrentPage);
        Assert.Equal(0, zeroRequest.PageSize);
        Assert.Equal(-1, negativeRequest.CurrentPage);
        Assert.Equal(-10, negativeRequest.PageSize);
    }

    [Fact]
    public void GetRequest_Id_EdgeCases()
    {
        // Arrange
        var minRequest = new GetRequest(int.MinValue, false);
        var maxRequest = new GetRequest(int.MaxValue, true);

        // Act & Assert
        Assert.Equal(int.MinValue, minRequest.Id);
        Assert.Equal(int.MaxValue, maxRequest.Id);
    }
}
