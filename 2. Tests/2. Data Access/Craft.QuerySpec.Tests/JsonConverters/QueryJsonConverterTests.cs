using System.Text.Json;
using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Converters;

public class QueryJsonConverterTests
{
    // Cache and reuse JsonSerializerOptions instances for each test type
    private static readonly JsonSerializerOptions CompanyOptions = CreateCompanyOptions();
    private static readonly JsonSerializerOptions CompanyCompanyOptions = CreateCompanyCompanyOptions();
    private static readonly JsonSerializerOptions CompanyCompanyNameOptions = CreateCompanyCompanyNameOptions();

    private static JsonSerializerOptions CreateCompanyOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new QueryJsonConverter<Company>());
        return options;
    }

    private static JsonSerializerOptions CreateCompanyCompanyOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new QueryJsonConverter<Company, Company>());
        return options;
    }

    private static JsonSerializerOptions CreateCompanyCompanyNameOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new QueryJsonConverter<Company, CompanyName>());
        return options;
    }

    [Fact]
    public void SerializeDeserialize_SimpleQuery_T_PreservesValues()
    {
        // Arrange
        var query = new Query<Company>
        {
            AsNoTracking = true,
            Skip = 10
        };
        query?.EntityFilterBuilder?.Add(c => c.Name == "John");
        query?.SortOrderBuilder?.Add(c => c.Id);

        // Act
        var serializedQuery = JsonSerializer.Serialize(query, CompanyOptions);
        var deserializedQuery = JsonSerializer.Deserialize<Query<Company>>(serializedQuery, CompanyOptions);

        // Assert
        Assert.NotNull(deserializedQuery);
        Assert.Equal(1, deserializedQuery.EntityFilterBuilder?.Count);
        Assert.Equal(1, deserializedQuery.SortOrderBuilder?.OrderDescriptorList.Count);
        Assert.True(deserializedQuery.AsNoTracking);
        Assert.Equal(10, deserializedQuery.Skip);
    }

    [Fact]
    public void SerializeDeserialize_SimpleQuery_T_TResult_PreservesValues()
    {
        // Arrange
        var query = new Query<Company, Company>
        {
            AsNoTracking = true,
            Skip = 10
        };
        query?.EntityFilterBuilder?.Add(c => c.Name == "John");
        query?.QuerySelectBuilder?.Add(c => c.Name!);
        query?.SortOrderBuilder?.Add(c => c.Id);

        // Act
        var serializedQuery = JsonSerializer.Serialize(query, CompanyCompanyOptions);
        var deserializedQuery = JsonSerializer.Deserialize<Query<Company, Company>>(serializedQuery, CompanyCompanyOptions);

        // Assert
        Assert.NotNull(deserializedQuery);
        Assert.Equal(1, deserializedQuery.EntityFilterBuilder?.Count);
        Assert.Equal(1, deserializedQuery.SortOrderBuilder?.OrderDescriptorList.Count);
        Assert.True(deserializedQuery.AsNoTracking);
        Assert.Equal(10, deserializedQuery.Skip);
        Assert.Equal(1, deserializedQuery.QuerySelectBuilder?.Count);
    }

    [Fact]
    public void SerializeDeserialize_QueryWithNullBuilders_DoesNotThrow()
    {
        // Arrange
        var query = new Query<Company>
        {
            AsNoTracking = false,
            SortOrderBuilder = null,
            EntityFilterBuilder = null,
            SqlLikeSearchCriteriaBuilder = null,
            Skip = null,
            Take = null
        };

        // Act
        var json = JsonSerializer.Serialize(query, CompanyOptions);
        var deserialized = JsonSerializer.Deserialize<Query<Company>>(json, CompanyOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.SortOrderBuilder);
        Assert.Null(deserialized.EntityFilterBuilder);
        Assert.Null(deserialized.SqlLikeSearchCriteriaBuilder);
        Assert.False(deserialized.AsNoTracking);
        Assert.Null(deserialized.Skip);
        Assert.Null(deserialized.Take);
    }

    [Fact]
    public void SerializeDeserialize_QueryWithAllDefaults_RoundTrips()
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        var json = JsonSerializer.Serialize(query, CompanyOptions);
        var deserialized = JsonSerializer.Deserialize<Query<Company>>(json, CompanyOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.True(deserialized.AsNoTracking);
        Assert.True(deserialized.IgnoreAutoIncludes);
        Assert.False(deserialized.AsSplitQuery);
        Assert.False(deserialized.IgnoreQueryFilters);
        Assert.Equal(query.Skip, deserialized.Skip);
        Assert.Equal(query.Take, deserialized.Take);
    }

    [Fact]
    public void SerializeDeserialize_QueryTResult_WithNullQuerySelectBuilder_DoesNotThrow()
    {
        // Arrange
        var query = new Query<Company, Company>
        {
            QuerySelectBuilder = null
        };

        // Act
        var json = JsonSerializer.Serialize(query, CompanyCompanyOptions);
        var deserialized = JsonSerializer.Deserialize<Query<Company, Company>>(json, CompanyCompanyOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(0, deserialized?.QuerySelectBuilder?.Count);
    }

    [Fact]
    public void SerializeDeserialize_QueryWithAllBooleansFalse()
    {
        // Arrange
        var query = new Query<Company>
        {
            AsNoTracking = false,
            AsSplitQuery = false,
            IgnoreAutoIncludes = false,
            IgnoreQueryFilters = false
        };

        // Act
        var json = JsonSerializer.Serialize(query, CompanyOptions);
        var deserialized = JsonSerializer.Deserialize<Query<Company>>(json, CompanyOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.False(deserialized.AsNoTracking);
        Assert.False(deserialized.AsSplitQuery);
        Assert.False(deserialized.IgnoreAutoIncludes);
        Assert.False(deserialized.IgnoreQueryFilters);
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        // Arrange
#pragma warning disable JSON001 // Invalid JSON pattern
        var invalidJson = "{ invalid json }";
#pragma warning restore JSON001 // Invalid JSON pattern

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Query<Company>>(invalidJson, CompanyOptions));
    }

    [Fact]
    public void Deserialize_MissingProperties_UsesDefaults()
    {
        // Arrange
        var json = "{}";

        // Act
        var deserialized = JsonSerializer.Deserialize<Query<Company>>(json, CompanyOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.True(deserialized.AsNoTracking);
        Assert.True(deserialized.IgnoreAutoIncludes);
        Assert.False(deserialized.AsSplitQuery);
        Assert.False(deserialized.IgnoreQueryFilters);
    }

    [Fact]
    public void SerializeDeserialize_QueryWithNegativeSkip_Throws()
    {
        // Arrange
        var query = new Query<Company>();
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Skip = -1);
    }

    [Fact]
    public void SerializeDeserialize_QueryWithNegativeTake_Throws()
    {
        // Arrange
        var query = new Query<Company>();
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Take = -1);
    }

    [Fact]
    public void SerializeDeserialize_QueryTResult_WithComplexQuerySelectBuilder_RoundTrips()
    {
        // Arrange
        var query = new Query<Company, CompanyName>();
        query?.QuerySelectBuilder?.Add("Name", "Name");
        query?.EntityFilterBuilder?.Add(c => c.Name == "Test");
        query?.SortOrderBuilder?.Add(c => c.Name!);

        // Act
        var json = JsonSerializer.Serialize(query, CompanyCompanyNameOptions);
        var deserialized = JsonSerializer.Deserialize<Query<Company, CompanyName>>(json, CompanyCompanyNameOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.QuerySelectBuilder);
        Assert.Equal(1, deserialized.QuerySelectBuilder.Count);
        Assert.NotNull(deserialized.EntityFilterBuilder);
        Assert.Equal(1, deserialized.EntityFilterBuilder.Count);
        Assert.NotNull(deserialized.SortOrderBuilder);
        Assert.Equal(1, deserialized.SortOrderBuilder?.OrderDescriptorList?.Count);
    }

    [Fact]
    public void Deserialize_ExtraProperties_IgnoresThem()
    {
        // Arrange
        var json = "{\"AsNoTracking\":true,\"ExtraProperty\":123,\"Skip\":5}";

        // Act
        var deserialized = JsonSerializer.Deserialize<Query<Company>>(json, CompanyOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.True(deserialized.AsNoTracking);
        Assert.Equal(5, deserialized.Skip);
    }

    [Fact]
    public void SerializeDeserialize_QueryWithNullsInBuilders_RoundTrips()
    {
        // Arrange
        var query = new Query<Company>
        {
            SortOrderBuilder = null,
            EntityFilterBuilder = null,
            SqlLikeSearchCriteriaBuilder = null
        };

        // Act
        var json = JsonSerializer.Serialize(query, CompanyOptions);
        var deserialized = JsonSerializer.Deserialize<Query<Company>>(json, CompanyOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.SortOrderBuilder);
        Assert.Null(deserialized.EntityFilterBuilder);
        Assert.Null(deserialized.SqlLikeSearchCriteriaBuilder);
    }

    [Fact]
    public void SerializeDeserialize_QueryTResult_WithNullQuerySelectBuilder_RoundTrips()
    {
        // Arrange
        var query = new Query<Company, CompanyName>
        {
            QuerySelectBuilder = null
        };

        // Act
        var json = JsonSerializer.Serialize(query, CompanyCompanyNameOptions);
        var deserialized = JsonSerializer.Deserialize<Query<Company, CompanyName>>(json, CompanyCompanyNameOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(0, deserialized.QuerySelectBuilder?.Count);
    }

    [Fact]
    public void SerializeDeserialize_QueryWithSqlLikeSearchCriteriaBuilder_RoundTrips()
    {
        // Arrange
        var query = new Query<Company>();
        query?.SqlLikeSearchCriteriaBuilder?.Add(c => c.Name!, "test");

        // Act
        var json = JsonSerializer.Serialize(query, CompanyOptions);
        var deserialized = JsonSerializer.Deserialize<Query<Company>>(json, CompanyOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.SqlLikeSearchCriteriaBuilder);
        Assert.Equal(1, deserialized.SqlLikeSearchCriteriaBuilder.Count);
    }

    [Fact]
    public void SerializeDeserialize_QueryWithNullOptions_Throws()
    {
        // Arrange
        var query = new Query<Company>();
        var converter = new QueryJsonConverter<Company>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => converter.Write(null!, query, null!));
    }

    [Fact]
    public void SerializeDeserialize_QueryTResult_WithNullOptions_Throws()
    {
        // Arrange
        var query = new Query<Company, CompanyName>();
        var converter = new QueryJsonConverter<Company, CompanyName>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => converter.Write(null!, query, null!));
    }
}
