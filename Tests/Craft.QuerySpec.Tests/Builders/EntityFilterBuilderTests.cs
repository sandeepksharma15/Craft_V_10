using System.Data;
using System.Linq.Expressions;
using System.Text.Json;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Builders;

public class EntityFilterBuilderTests
{
    private readonly JsonSerializerOptions serializeOptions;
    private readonly IQueryable<Company> queryable;

    public EntityFilterBuilderTests()
    {
        serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new EntityFilterBuilderJsonConverter<Company>());

        queryable = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 2, Name = "Company 2" }
        }.AsQueryable();
    }

    [Fact]
    public void Clear_EmptiesWhereExpressions()
    {
        // Arrange
        var whereBuilder = new EntityFilterBuilder<Company>();
        whereBuilder.Add(u => u.Name == "Company 1");

        // Act
        var result = whereBuilder.Clear();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(whereBuilder, result);
        Assert.Empty(whereBuilder.EntityFilterList);
    }

    [Fact]
    public void Add_Expression_AddsToWhereExpressions()
    {
        // Arrange
        var whereBuilder = new EntityFilterBuilder<Company>();
        Expression<Func<Company, bool>> expression = u => u.Name == "Company 1";

        // Act
        var result = whereBuilder.Add(expression);
        var expr = whereBuilder.EntityFilterList[0];
        var filtered = queryable.Where(expr.Filter).ToList();

        // Assert
        Assert.NotNull(filtered);
        Assert.Single(filtered);
        Assert.Equal("Company 1", filtered[0].Name);
        Assert.Equal(whereBuilder, result);
    }

    [Fact]
    public void Add_ExpressionWithProperties_ReturnsWhereExpressions()
    {
        // Arrange
        var whereBuilder = new EntityFilterBuilder<Company>();
        Expression<Func<Company, object>> propExpr = u => u.Name!;
        object compareWith = "Company 1";

        // Act
        var result = whereBuilder.Add(propExpr, compareWith, ComparisonType.EqualTo);
        var expr = whereBuilder.EntityFilterList[0];
        var filtered = queryable.Where(expr.Filter).ToList();

        // Assert
        Assert.NotNull(filtered);
        Assert.Single(filtered);
        Assert.Equal("Company 1", filtered[0].Name);
        Assert.Equal(whereBuilder, result);
        Assert.NotEmpty(whereBuilder.EntityFilterList);
    }

    [Fact]
    public void Add_StringPropName_ReturnsWhereExpressions()
    {
        // Arrange
        var whereBuilder = new EntityFilterBuilder<Company>();
        const string propName = "Name";
        object compareWith = "Company 1";

        // Act
        var result = whereBuilder.Add(propName, compareWith, ComparisonType.NotEqualTo);
        var expr = whereBuilder.EntityFilterList[0];
        var filtered = queryable.Where(expr.Filter).ToList();

        // Assert
        Assert.NotNull(filtered);
        Assert.Single(filtered);
        Assert.Equal("Company 2", filtered[0].Name);
        Assert.Equal(whereBuilder, result);
        Assert.NotEmpty(whereBuilder.EntityFilterList);
    }

    [Fact]
    public void Remove_WhenExpressionFound_ShouldRemoveFromWhereExpressions()
    {
        // Arrange
        var whereBuilder = new EntityFilterBuilder<Company>();
        Expression<Func<Company, bool>> expression = t => t.Id == 1;
        whereBuilder.Add(expression);

        // Act
        whereBuilder.Remove(expression);

        // Assert
        Assert.NotNull(whereBuilder);
        Assert.Empty(whereBuilder.EntityFilterList);
    }

    [Fact]
    public void Remove_WhenExpressionNotFound_ShouldNotChangeWhereExpressions()
    {
        // Arrange
        var whereBuilder = new EntityFilterBuilder<Company>();
        Expression<Func<Company, bool>> expression = t => t.Id == 1;
        whereBuilder.Add(t => t.Name == "Test");

        // Act
        whereBuilder.Remove(expression);

        // Assert
        Assert.NotNull(whereBuilder);
        Assert.NotEmpty(whereBuilder.EntityFilterList);
    }

    [Fact]
    public void Remove_WhenPropertyExpressionFound_ShouldRemoveFromWhereExpressions()
    {
        // Arrange
        var whereBuilder = new EntityFilterBuilder<Company>();
        Expression<Func<Company, object>> propExpr = u => u.Name!;
        object compareWith = "Company 1";
        whereBuilder.Add(propExpr, compareWith, ComparisonType.EqualTo);

        // Act
        whereBuilder.Remove(propExpr, compareWith, ComparisonType.EqualTo);

        // Assert
        Assert.NotNull(whereBuilder);
        Assert.Empty(whereBuilder.EntityFilterList);
    }

    [Fact]
    public void Remove_WhenPropertyNameExpressionFound_ShouldRemoveFromWhereExpressions()
    {
        // Arrange
        var whereBuilder = new EntityFilterBuilder<Company>();
        const string propName = "Name";
        object compareWith = "Company 1";

        whereBuilder.Add(propName, compareWith, ComparisonType.NotEqualTo);

        // Act
        whereBuilder.Remove(propName, compareWith, ComparisonType.NotEqualTo);

        // Assert
        Assert.NotNull(whereBuilder);
        Assert.Empty(whereBuilder.EntityFilterList);
    }

    [Fact]
    public void CanConvert_ReturnsTrueForCorrectType()
    {
        // Arrange
        var converter = new EntityFilterBuilderJsonConverter<TestClass>();
        bool canConvert = converter.CanConvert(typeof(EntityFilterBuilder<TestClass>));

        // Assert
        Assert.True(canConvert);
    }

    [Fact]
    public void CanConvert_ReturnsFalseForIncorrectType()
    {
        // Arrange
        var converter = new EntityFilterBuilderJsonConverter<TestClass>();
        bool canConvert = converter.CanConvert(typeof(string));

        // Assert
        Assert.False(canConvert);
    }

    [Fact]
    public void Read_DeserializesValidJsonToEntityFilterBuilder()
    {
        // Arrange
        const string validJson = "[{\"Filter\": \"Name == 'John'\"}]";

        // Act
        var filterBuilder = JsonSerializer.Deserialize<EntityFilterBuilder<Company>>(validJson, serializeOptions);

        // Assert
        Assert.NotNull(filterBuilder);
        Assert.NotEmpty(filterBuilder.EntityFilterList);
        Assert.Single(filterBuilder.EntityFilterList);
        Assert.Contains(filterBuilder.EntityFilterList, f => f.Filter.Body.ToString().Contains("x.Name"));
    }

    [Fact]
    public void Read_ThrowsJsonExceptionForInvalidFormat()
    {
        // Arrange
        const string invalidJson = "{}"; // Not an array

        // Act and Assert
        Assert.Throws<JsonException>(() => JsonSerializer
            .Deserialize<EntityFilterBuilder<Company>>(invalidJson, serializeOptions));
    }

    [Fact]
    public void Write_SerializesEntityFilterBuilderToJsonCorrectly()
    {
        // Arrange
        var filterBuilder = new EntityFilterBuilder<Company>();
        Expression<Func<Company, bool>> filterCriteria = x => x.Name == "John";
        filterBuilder.Add(filterCriteria);

        // Act
        var json = JsonSerializer.Serialize(filterBuilder, serializeOptions);

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Equal("[{\"Filter\":\"(Name == \\u0022John\\u0022)\"}]", json);
    }

    [Fact]
    public void Write_SerializesEmptyEntityFilterBuilderToJsonCorrectly()
    {
        // Arrange
        var filterBuilder = new EntityFilterBuilder<Company>();

        // Act
        var json = JsonSerializer.Serialize(filterBuilder, serializeOptions);

        // Assert
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Read_DeserializesEmptyArrayToEntityFilterBuilder()
    {
        // Arrange
        const string emptyJson = "[]";

        // Act
        var filterBuilder = JsonSerializer.Deserialize<EntityFilterBuilder<Company>>(emptyJson, serializeOptions);

        // Assert
        Assert.NotNull(filterBuilder);
        Assert.Empty(filterBuilder.EntityFilterList);
    }

    [Fact]
    public void Write_SerializesMultipleFiltersToJsonCorrectly()
    {
        // Arrange
        var filterBuilder = new EntityFilterBuilder<Company>();
        filterBuilder.Add(x => x.Name == "A");
        filterBuilder.Add(x => x.Id > 0);

        // Act
        var json = JsonSerializer.Serialize(filterBuilder, serializeOptions);

        // Assert
        Assert.Contains("Name", json);
        Assert.Contains("Id", json);
    }

    [Fact]
    public void Read_ThrowsJsonExceptionForNonArrayToken()
    {
        // Arrange
        const string notArrayJson = "{\"Filter\":\"Name == 'A'\"}";

        // Act and Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterBuilder<Company>>(notArrayJson, serializeOptions));
    }

    [Fact]
    public void Read_ThrowsJsonExceptionForInvalidFilterCriteria()
    {
        // Invalid: missing Filter property
        const string invalidCriteriaJson = "[{\"Invalid\":123}]";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterBuilder<Company>>(invalidCriteriaJson, serializeOptions));
    }

    [Fact]
    public void Write_SerializesComplexFilterToJsonCorrectly()
    {
        // Arrange
        var filterBuilder = new EntityFilterBuilder<Company>();

        filterBuilder.Add(x => x.Name != "B");
        filterBuilder.Add(x => x.Name!.Contains('C'));

        // Act
        var json = JsonSerializer.Serialize(filterBuilder, serializeOptions);

        // Assert
        Assert.Contains("!=", json);
        Assert.Contains("Contains", json);
    }

    [Fact]
    public void Add_NullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<Company, bool>>)null!));
    }

    [Fact]
    public void Add_NullPropertyExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<Company, object>>)null!, "Test", ComparisonType.EqualTo));
    }

    [Fact]
    public void Add_InvalidPropertyName_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add("InvalidProperty", "Test", ComparisonType.EqualTo));
    }

    [Fact]
    public void Remove_NullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Remove((Expression<Func<Company, bool>>)null!));
    }

    [Fact]
    public void Remove_NullPropertyExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Remove((Expression<Func<Company, object>>)null!, "Test", ComparisonType.EqualTo));
    }

    [Fact]
    public void Remove_InvalidPropertyName_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Remove("InvalidProperty", "Test", ComparisonType.EqualTo));
    }

    [Fact]
    public void Remove_OnEmptyBuilder_DoesNotThrow()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();
        var expr = (Expression<Func<Company, bool>>)(x => x.Name == "Test");

        // Act
        builder.Remove(expr);

        // Assert
        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Add_DuplicateExpressions_AddsBoth()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();
        Expression<Func<Company, bool>> expr = x => x.Name == "Test";

        // Act
        builder.Add(expr);
        builder.Add(expr);

        // Assert
        Assert.Equal(2, builder.EntityFilterList.Count);
    }

    [Fact]
    public void Clear_OnEmptyBuilder_DoesNotThrow()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();

        // Act
        builder.Clear();

        // Assert
        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Add_And_Remove_EnumProperty()
    {
        // Arrange
        // Assuming Company has an enum property, for demonstration we'll use Id as int
        var builder = new EntityFilterBuilder<Company>();
        builder.Add(x => x.Id == 1);

        // Act
        builder.Remove(x => x.Id == 1);

        // Assert
        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Serialize_NullEntityFilterBuilder_WritesJsonNull()
    {
        // Act
        var json = JsonSerializer.Serialize<EntityFilterBuilder<Company>>(null!, serializeOptions);

        // Assert
        Assert.Equal("null", json);
    }

    [Fact]
    public void Serialize_EntityFilterBuilderWithNullCriteria_ThrowsException()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();

        // Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(null!));
    }

    [Fact]
    public void Deserialize_ArrayWithNullElement_ThrowsJsonException()
    {
        // Arrange
        const string json = "[null]";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterBuilder<Company>>(json, serializeOptions));
    }

    [Fact]
    public void Deserialize_ArrayWithValidAndInvalidCriteria_ThrowsJsonExceptionOnFirstInvalid()
    {
        // Arrange: first is valid, second is invalid
        const string json = "[{\"Filter\":\"Name == 'A'\"},{\"Invalid\":123}]";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterBuilder<Company>>(json, serializeOptions));
    }

    [Fact]
    public void SerializeDeserialize_WithCustomPropertyNamingPolicy_WorksCorrectly()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();
        builder.Add(x => x.Name == "Test");
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new EntityFilterBuilderJsonConverter<Company>());

        // Act
        var json = JsonSerializer.Serialize(builder, options);
        var deserialized = JsonSerializer.Deserialize<EntityFilterBuilder<Company>>(json, options);

        // Assert
        Assert.NotNull(json);
        Assert.NotNull(deserialized);
        Assert.Single(deserialized.EntityFilterList);
        Assert.Contains("name", json, System.StringComparison.OrdinalIgnoreCase);
    }
}
