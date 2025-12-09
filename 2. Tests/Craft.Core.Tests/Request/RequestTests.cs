using System.Text.Json;
using Craft.Core.Request;

namespace Craft.Core.Tests.Request;

/// <summary>
/// Unit tests for APIRequest and its derived types, including serialization and deserialization, for both generic and non-generic usage.
/// </summary>
public class RequestTests
{
    [Fact]
    public void GetRequest_NonGeneric_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        long id = 42;
        bool includeDetails = true;

        // Act
        var request = new GetRequest(id, includeDetails);

        // Assert
        Assert.Equal(ApiRequestType.Get, request.RequestType);
        Assert.Equal(id, request.Id);
        Assert.Equal(includeDetails, request.IncludeDetails);
    }

    [Fact]
    public void GetRequest_GenericInt_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        int id = 123;
        bool includeDetails = false;

        // Act
        var request = new GetRequest<int>(id, includeDetails);

        // Assert
        Assert.Equal(ApiRequestType.Get, request.RequestType);
        Assert.Equal(id, request.Id);
        Assert.Equal(includeDetails, request.IncludeDetails);
    }

    [Fact]
    public void GetRequest_GenericGuid_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        bool includeDetails = true;

        // Act
        var request = new GetRequest<Guid>(id, includeDetails);

        // Assert
        Assert.Equal(ApiRequestType.Get, request.RequestType);
        Assert.Equal(id, request.Id);
        Assert.Equal(includeDetails, request.IncludeDetails);
    }

    [Fact]
    public void GetAllRequest_NonGeneric_Constructor_SetsPropertiesCorrectly()
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
    public void GetAllRequest_GenericString_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        bool includeDetails = true;

        // Act
        var request = new GetAllRequest<string>(includeDetails);

        // Assert
        Assert.Equal(ApiRequestType.GetAll, request.RequestType);
        Assert.Equal(includeDetails, request.IncludeDetails);
    }

    [Fact]
    public void GetPagedRequest_NonGeneric_Constructor_SetsPropertiesCorrectly()
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
    public void GetPagedRequest_GenericGuid_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        int currentPage = 2;
        int pageSize = 10;
        bool includeDetails = false;

        // Act
        var request = new GetPagedRequest<Guid>(currentPage, pageSize, includeDetails);

        // Assert
        Assert.Equal(ApiRequestType.GetPaged, request.RequestType);
        Assert.Equal(currentPage, request.CurrentPage);
        Assert.Equal(pageSize, request.PageSize);
        Assert.Equal(includeDetails, request.IncludeDetails);
    }

    [Fact]
    public void GetRequest_NonGeneric_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetRequest(7, true);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GetRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(7, deserialized.Id);
        Assert.True(deserialized.IncludeDetails);
        Assert.Equal(ApiRequestType.Get, deserialized.RequestType);
    }

    [Fact]
    public void GetRequest_GenericInt_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetRequest<int>(123, true);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GetRequest<int>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(123, deserialized.Id);
        Assert.True(deserialized.IncludeDetails);
        Assert.Equal(ApiRequestType.Get, deserialized.RequestType);
    }

    [Fact]
    public void GetRequest_GenericGuid_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var original = new GetRequest<Guid>(guid, false);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GetRequest<Guid>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(guid, deserialized.Id);
        Assert.False(deserialized.IncludeDetails);
        Assert.Equal(ApiRequestType.Get, deserialized.RequestType);
    }

    [Fact]
    public void GetAllRequest_NonGeneric_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetAllRequest(false);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GetAllRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.False(deserialized.IncludeDetails);
        Assert.Equal(ApiRequestType.GetAll, deserialized.RequestType);
    }

    [Fact]
    public void GetAllRequest_GenericString_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetAllRequest<string>(true);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GetAllRequest<string>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.True(deserialized.IncludeDetails);
        Assert.Equal(ApiRequestType.GetAll, deserialized.RequestType);
    }

    [Fact]
    public void GetPagedRequest_NonGeneric_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetPagedRequest(2, 50, true);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GetPagedRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.CurrentPage);
        Assert.Equal(50, deserialized.PageSize);
        Assert.True(deserialized.IncludeDetails);
        Assert.Equal(ApiRequestType.GetPaged, deserialized.RequestType);
    }

    [Fact]
    public void GetPagedRequest_GenericGuid_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetPagedRequest<Guid>(5, 20, false);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GetPagedRequest<Guid>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(5, deserialized.CurrentPage);
        Assert.Equal(20, deserialized.PageSize);
        Assert.False(deserialized.IncludeDetails);
        Assert.Equal(ApiRequestType.GetPaged, deserialized.RequestType);
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
        var minRequest = new GetRequest<int>(int.MinValue, false);
        var maxRequest = new GetRequest<int>(int.MaxValue, true);

        // Act & Assert
        Assert.Equal(int.MinValue, minRequest.Id);
        Assert.Equal(int.MaxValue, maxRequest.Id);
    }

    [Fact]
    public void GetRequest_GenericString_Serialization_And_Deserialization_Works()
    {
        // Arrange
        var original = new GetRequest<string>("abc", true);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GetRequest<string>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("abc", deserialized.Id);
        Assert.True(deserialized.IncludeDetails);
        Assert.Equal(ApiRequestType.Get, deserialized.RequestType);
    }

    [Fact]
    public void GetPagedRequest_GenericString_Constructor_And_Serialization_Works()
    {
        // Arrange
        var original = new GetPagedRequest<string>(7, 15, true);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GetPagedRequest<string>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(7, deserialized.CurrentPage);
        Assert.Equal(15, deserialized.PageSize);
        Assert.True(deserialized.IncludeDetails);
        Assert.Equal(ApiRequestType.GetPaged, deserialized.RequestType);
    }
}
