using System.Text.Json;

namespace Craft.QuerySpec.Tests.Converters;

public class FilterCriteriaJsonConverterTests
{
    [Fact]
    public void Deserialize_StringBackedValueObjectMetadata_PreservesRawStringValue()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new FilterCriteriaJsonConverter());

        var json = $$"""
        {
          "PropertyType": "{{typeof(TestCode).AssemblyQualifiedName}}",
          "Name": "Code",
          "Value": "123",
          "Comparison": 6,
          "DisplayTitle": "Code"
        }
        """;

        var criteria = JsonSerializer.Deserialize<FilterCriteria>(json, options);

        Assert.NotNull(criteria);
        Assert.Equal(typeof(TestCode), criteria.PropertyType);
        Assert.Equal("Code", criteria.Name);
        Assert.Equal("123", Assert.IsType<string>(criteria.Value));
        Assert.Equal(ComparisonType.Contains, criteria.Comparison);
    }

    [Fact]
    public void SerializeDeserialize_QueryWithStringBackedValueObjectFilter_RoundTrips()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new QueryJsonConverter<TestEntity>());

        var query = new Query<TestEntity>();
        query.EntityFilterBuilder?.Add(new FilterCriteria(typeof(TestCode), nameof(TestEntity.Code), "123", ComparisonType.Contains));

        var json = JsonSerializer.Serialize(query, options);
        var deserializedQuery = JsonSerializer.Deserialize<Query<TestEntity>>(json, options);

        var filter = Assert.Single(deserializedQuery?.EntityFilterBuilder?.EntityFilterList ?? []);
        Assert.NotNull(filter.Metadata);
        Assert.Equal(typeof(TestCode), filter.Metadata.PropertyType);
        Assert.Equal("123", Assert.IsType<string>(filter.Metadata.Value));
        Assert.Equal(ComparisonType.Contains, filter.Metadata.Comparison);
    }

    private sealed class TestEntity
    {
        public TestCode Code { get; set; }
    }

    private readonly record struct TestCode(string Value);
}
