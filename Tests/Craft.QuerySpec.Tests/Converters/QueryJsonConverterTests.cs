using System.Text.Json;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Converters;

public class QueryJsonConverterTests
{
    [Fact]
    public void SerializeDeserialize_SimpleQuery_T_PreservesValues()
    {
        // Arrange
        var query = new Query<Company>
        {
            AsNoTracking = true,
            Skip = 10
        };
        query.Where(c => c.Name == "John");
        query.OrderBy(c => c.Id);

        // Act
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new QueryJsonConverter<Company>());

        var serializedQuery = JsonSerializer.Serialize(query, serializeOptions);
        var deserializedQuery = JsonSerializer.Deserialize<Query<Company>>(serializedQuery, serializeOptions);

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
        query.Where(c => c.Name == "John");
        query.Select(c => c.Name!);
        query.OrderBy(c => c.Id);

        // Act
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new QueryJsonConverter<Company, Company>());

        var serializedQuery = JsonSerializer.Serialize(query, serializeOptions);

        var deserializedQuery = JsonSerializer
            .Deserialize<Query<Company, Company>>(serializedQuery, serializeOptions);

        // Assert
        Assert.NotNull(deserializedQuery);
        Assert.Equal(1, deserializedQuery.EntityFilterBuilder?.Count);
        Assert.Equal(1, deserializedQuery.SortOrderBuilder?.OrderDescriptorList.Count);
        Assert.True(deserializedQuery.AsNoTracking);
        Assert.Equal(10, deserializedQuery.Skip);
        Assert.Equal(1, deserializedQuery.QuerySelectBuilder?.Count);
    }
}
