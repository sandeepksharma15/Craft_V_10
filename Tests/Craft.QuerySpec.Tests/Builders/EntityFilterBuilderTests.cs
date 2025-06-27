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
        var converter = new EntityFilterBuilderJsonConverter<TestClass>();
        bool canConvert = converter.CanConvert(typeof(EntityFilterBuilder<TestClass>));

        Assert.True(canConvert);
    }

    [Fact]
    public void CanConvert_ReturnsFalseForIncorrectType()
    {
        var converter = new EntityFilterBuilderJsonConverter<TestClass>();
        bool canConvert = converter.CanConvert(typeof(string));

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
        var filterBuilder = new EntityFilterBuilder<Company>();
        var json = JsonSerializer.Serialize(filterBuilder, serializeOptions);
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Read_DeserializesEmptyArrayToEntityFilterBuilder()
    {
        const string emptyJson = "[]";
        var filterBuilder = JsonSerializer.Deserialize<EntityFilterBuilder<Company>>(emptyJson, serializeOptions);
        Assert.NotNull(filterBuilder);
        Assert.Empty(filterBuilder.EntityFilterList);
    }

    [Fact]
    public void Write_SerializesMultipleFiltersToJsonCorrectly()
    {
        var filterBuilder = new EntityFilterBuilder<Company>();
        filterBuilder.Add(x => x.Name == "A");
        filterBuilder.Add(x => x.Id > 0);
        var json = JsonSerializer.Serialize(filterBuilder, serializeOptions);
        Assert.Contains("Name", json);
        Assert.Contains("Id", json);
    }

    [Fact]
    public void Read_ThrowsJsonExceptionForNonArrayToken()
    {
        const string notArrayJson = "{\"Filter\":\"Name == 'A'\"}";
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
        var filterBuilder = new EntityFilterBuilder<Company>();

        filterBuilder.Add(x => x.Name != "B");
        filterBuilder.Add(x => x.Name!.Contains('C'));

        var json = JsonSerializer.Serialize(filterBuilder, serializeOptions);

        Assert.Contains("!=", json);
        Assert.Contains("Contains", json);
    }

    [Fact]
    public void Add_NullExpression_ThrowsArgumentNullException()
    {
        var builder = new EntityFilterBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<Company, bool>>)null!));
    }

    [Fact]
    public void Add_NullPropertyExpression_ThrowsArgumentNullException()
    {
        var builder = new EntityFilterBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<Company, object>>)null!, "Test", ComparisonType.EqualTo));
    }

    [Fact]
    public void Add_InvalidPropertyName_ThrowsArgumentNullException()
    {
        var builder = new EntityFilterBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Add("InvalidProperty", "Test", ComparisonType.EqualTo));
    }

    [Fact]
    public void Remove_NullExpression_ThrowsArgumentNullException()
    {
        var builder = new EntityFilterBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove((Expression<Func<Company, bool>>)null!));
    }

    [Fact]
    public void Remove_NullPropertyExpression_ThrowsArgumentNullException()
    {
        var builder = new EntityFilterBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove((Expression<Func<Company, object>>)null!, "Test", ComparisonType.EqualTo));
    }

    [Fact]
    public void Remove_InvalidPropertyName_ThrowsArgumentNullException()
    {
        var builder = new EntityFilterBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove("InvalidProperty", "Test", ComparisonType.EqualTo));
    }

    [Fact]
    public void Remove_OnEmptyBuilder_DoesNotThrow()
    {
        var builder = new EntityFilterBuilder<Company>();
        var expr = (Expression<Func<Company, bool>>)(x => x.Name == "Test");
        builder.Remove(expr);
        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Add_DuplicateExpressions_AddsBoth()
    {
        var builder = new EntityFilterBuilder<Company>();
        Expression<Func<Company, bool>> expr = x => x.Name == "Test";
        builder.Add(expr);
        builder.Add(expr);
        Assert.Equal(2, builder.EntityFilterList.Count);
    }

    [Fact]
    public void Clear_OnEmptyBuilder_DoesNotThrow()
    {
        var builder = new EntityFilterBuilder<Company>();
        builder.Clear();
        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Add_And_Remove_EnumProperty()
    {
        // Assuming Company has an enum property, for demonstration we'll use Id as int
        var builder = new EntityFilterBuilder<Company>();
        builder.Add(x => x.Id == 1);
        builder.Remove(x => x.Id == 1);
        Assert.Empty(builder.EntityFilterList);
    }
}
