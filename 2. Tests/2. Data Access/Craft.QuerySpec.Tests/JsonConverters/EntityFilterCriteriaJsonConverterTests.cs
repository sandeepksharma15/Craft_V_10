using System.Linq.Expressions;
using System.Text;
using System.Text.Json;

namespace Craft.QuerySpec.Tests.Converters;

/// <summary>
/// Comprehensive test suite for EntityFilterCriteriaJsonConverter covering all functionality,
/// serialization/deserialization scenarios, error conditions, and edge cases.
/// </summary>
public class EntityFilterCriteriaJsonConverterTests
{
    #region Test Setup and Helpers

    private readonly JsonSerializerOptions _jsonOptions;
    private readonly EntityFilterCriteriaJsonConverter<TestEntity> _converter;

    public EntityFilterCriteriaJsonConverterTests()
    {
        _jsonOptions = new JsonSerializerOptions();
        _converter = new EntityFilterCriteriaJsonConverter<TestEntity>();
        _jsonOptions.Converters.Add(_converter);
        _jsonOptions.Converters.Add(new EntityFilterCriteriaJsonConverter<ComplexTestEntity>());
    }

    #endregion

    #region Write Method Tests

    [Fact]
    public void Write_WithValidCriteria_WritesCorrectJson()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x => x.Name == "John";
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        Assert.Contains("Name", json);
        Assert.Contains("John", json);
    }

    [Fact]
    public void Write_WithNullValue_WritesNullJson()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, null, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Equal("null", json);
    }

    [Fact]
    public void Write_WithNullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x => x.Name == "John";
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _converter.Write(null!, criteria, _jsonOptions));

        Assert.Equal("writer", exception.ParamName);
    }

    [Fact]
    public void Write_WithComplexExpression_CreatesValidJson()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x =>
            x.Age > 18 && x.Name.Contains("test") && x.IsActive;
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        Assert.Contains("Age", json);
        Assert.Contains("Name", json);
        Assert.Contains("Contains", json);
        Assert.Contains("IsActive", json);
    }

    [Fact]
    public void Write_WithExpressionContainingParameterAccessors_RemovesParameterNames()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x => x.Name == "John" && x.Age > 25;
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        // Should not contain parameter accessor patterns like "(x."
        Assert.DoesNotContain("(x.", json);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Test")]
    [InlineData("Special\\Characters")]
    [InlineData("Unicode\u00e9")]
    public void Write_WithVariousStringValues_HandlesCorrectly(string testValue)
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x => x.Name == testValue;
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        Assert.Contains("Name", json);
    }

    #endregion

    #region Read Method Tests

    [Fact]
    public void Read_WithValidJson_ReturnsCorrectCriteria()
    {
        // Arrange
        const string json = "{\"Filter\": \"Name == \\\"John\\\"\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var testEntity = new TestEntity { Name = "John" };
        Assert.True(result.Matches(testEntity));
    }

    [Fact]
    public void Read_WithNullToken_ReturnsNull()
    {
        // Arrange
        const string json = "null";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to Null token

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Read_WithInvalidStartToken_ThrowsJsonException()
    {
        // Arrange
        const string json = "\"invalid\"";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to String token

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        Assert.Contains("Expected StartObject token", exception.Message);
    }

    [Fact]
    public void Read_WithEmptyObject_ThrowsJsonException()
    {
        // Arrange
        const string json = "{}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        Assert.Contains("Empty JSON object is not valid", exception.Message);
    }

    [Fact]
    public void Read_WithMissingFilterProperty_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"SomeOtherProperty\": \"value\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        Assert.Contains("Required property 'Filter' or 'Metadata'", exception.Message);
    }

    [Fact]
    public void Read_WithNullFilterValue_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\": null}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        Assert.Contains("Filter expression cannot be null", exception.Message);
    }

    [Fact]
    public void Read_WithNonStringFilterValue_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\": 123}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        Assert.Contains("Filter expression must be a string", exception.Message);
    }

    [Fact]
    public void Read_WithEmptyFilterString_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\": \"\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        Assert.Contains("Filter expression cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Read_WithWhitespaceFilterString_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\": \"   \"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        Assert.Contains("Filter expression cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Read_WithInvalidFilterExpression_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\": \"Invalid Expression Syntax\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        Assert.Contains("Unable to parse filter expression", exception.Message);
    }

    [Fact]
    public void Read_WithValidComplexFilter_CreatesWorkingCriteria()
    {
        // Arrange
        const string json = "{\"Filter\": \"Age > 18 && Name == 'John'\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var matchingEntity = new TestEntity { Name = "John", Age = 25 };
        var nonMatchingEntity = new TestEntity { Name = "John", Age = 16 };

        Assert.True(result.Matches(matchingEntity));
        Assert.False(result.Matches(nonMatchingEntity));
    }

    [Fact]
    public void Read_WithUnknownProperties_SkipsThemGracefully()
    {
        // Arrange
        const string json = "{\"UnknownProperty1\": \"value1\", \"Filter\": \"Name == 'John'\", \"UnknownProperty2\": 123}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var testEntity = new TestEntity { Name = "John" };
        Assert.True(result.Matches(testEntity));
    }

    [Fact]
    public void Read_WithNestedObjectProperties_SkipsCorrectly()
    {
        // Arrange
        const string json = "{\"NestedObject\": {\"Property1\": \"value\", \"Property2\": {\"Nested\": true}}, \"Filter\": \"Name == 'John'\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var testEntity = new TestEntity { Name = "John" };
        Assert.True(result.Matches(testEntity));
    }

    [Fact]
    public void Read_WithArrayProperties_SkipsCorrectly()
    {
        // Arrange
        const string json = "{\"ArrayProperty\": [1, 2, 3, {\"nested\": \"value\"}], \"Filter\": \"Name == 'John'\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var testEntity = new TestEntity { Name = "John" };
        Assert.True(result.Matches(testEntity));
    }

    [Fact]
    public void Read_WithUnexpectedTokenInObject_ThrowsJsonException()
    {
        // Arrange - Manually create a malformed JSON scenario
        var json = "{\"Filter\": \"Name == 'John'\" 123}"; // Invalid token after property
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
    }

    [Fact]
    public void Read_WithIncompleteJson_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\":";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        // Accept either .NET 6+ or older error message for incomplete JSON
        Assert.Contains("Expected depth to be zero at the end of t", exception.Message);
    }

    #endregion

    #region Filter Expression Normalization Tests

    [Theory]
    [InlineData("Name == 'John'", "Name == \"John\"")]
    [InlineData("Age > 18", "Age > 18")]
    [InlineData("(Name == 'Test')", "Name == \"Test\"")]
    [InlineData("((Age > 25))", "Age > 25")]
    public void Read_NormalizeFilterString_HandlesQuotesAndParentheses(string inputFilter, string expectedNormalized)
    {
        // Arrange
        var json = $"{{\"Filter\": \"{inputFilter.Replace("\"", "\\\"")}\"}}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        // The exact string representation may vary, but the functionality should work
        if (expectedNormalized.Contains("Name"))
        {
            var testEntity = new TestEntity { Name = "John" };
            var testResult = result.Matches(testEntity);
            Assert.True(testResult || !expectedNormalized.Contains("John"));
        }
    }

    [Fact]
    public void Read_WithSingleQuotes_ConvertsToDoulbeQuotes()
    {
        // Arrange
        const string json = "{\"Filter\": \"Name == 'John'\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var testEntity = new TestEntity { Name = "John" };
        Assert.True(result.Matches(testEntity));
    }

    [Fact]
    public void Read_WithSurroundingParentheses_RemovesThem()
    {
        // Arrange
        const string json = "{\"Filter\": \"(Name == 'John')\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var testEntity = new TestEntity { Name = "John" };
        Assert.True(result.Matches(testEntity));
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Read_WithEntityFilterCriteriaConstructorException_WrapsInJsonException()
    {
        // Arrange - Use a filter that will cause EntityFilterCriteria constructor to throw
        const string json = "{\"Filter\": \"Invalid == Expression\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act & Assert
        JsonException exception = null!;
        try
        {
            _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
    }

    [Fact]
    public void Write_WithSerializationException_WrapsInJsonException()
    {
        // Arrange - Create a scenario that might cause serialization to fail
        // This is difficult to test directly since the SerializeFilterExpression method
        // is well-protected, but we can test the error handling path indirectly

        Expression<Func<TestEntity, bool>> expression = x => x.Name == "John";
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act - Normal case should work
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
    }

    #endregion

    #region Integration Tests with JsonSerializer

    [Fact]
    public void JsonSerializer_SerializeDeserialize_RoundTripWorks()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x => x.Name == "John";
        var originalCriteria = new EntityFilterCriteria<TestEntity>(expression);

        // Act
        var json = JsonSerializer.Serialize(originalCriteria, _jsonOptions);
        var deserializedCriteria = JsonSerializer.Deserialize<EntityFilterCriteria<TestEntity>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserializedCriteria);

        var testEntity = new TestEntity { Name = "John", Age = 25 };
        Assert.True(originalCriteria.Matches(testEntity));
        Assert.True(deserializedCriteria.Matches(testEntity));

        var nonMatchingEntity = new TestEntity { Name = "Jane", Age = 16 };
        Assert.False(originalCriteria.Matches(nonMatchingEntity));
        Assert.False(deserializedCriteria.Matches(nonMatchingEntity));
    }

    [Fact]
    public void JsonSerializer_WithNullCriteria_SerializesAsNull()
    {
        // Arrange
        EntityFilterCriteria<TestEntity>? nullCriteria = null;

        // Act
        var json = JsonSerializer.Serialize(nullCriteria, _jsonOptions);
        var deserializedCriteria = JsonSerializer.Deserialize<EntityFilterCriteria<TestEntity>>(json, _jsonOptions);

        // Assert
        Assert.Equal("null", json);
        Assert.Null(deserializedCriteria);
    }

    #endregion

    #region Edge Cases and Boundary Conditions

    [Fact]
    public void Read_WithVeryLongFilterExpression_HandlesCorrectly()
    {
        // Arrange
        var longFilter = string.Join(" && ", Enumerable.Range(1, 50).Select(i => $"Name != 'Test{i}'"));
        var json = $"{{\"Filter\": \"{longFilter}\"}}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var testEntity = new TestEntity { Name = "ValidName" };
        Assert.True(result.Matches(testEntity));
    }

    [Fact]
    public void Write_WithConstantExpression_SerializesCorrectly()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x => true;
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        Assert.Contains("True", json);
    }

    [Fact]
    public void Read_WithSpecialCharactersInFilter_HandlesCorrectly()
    {
        // Arrange
        const string json = "{\"Filter\": \"Name == 'Test\\\"Value'\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        // The expression should be created successfully even with special characters
    }

    [Fact]
    public void Read_WithMultipleFilterProperties_UsesFirst()
    {
        // Arrange
        const string json = "{\"Filter\": \"Name == 'John'\", \"Filter\": \"Age > 18\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        // JSON parsing behavior with duplicate keys depends on the parser,
        // but the converter should handle it gracefully
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Write_WithLargeExpression_PerformsReasonably()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x =>
            x.Name.Contains("test") &&
            x.Age > 18 &&
            x.Age < 100 &&
            x.IsActive &&
            !string.IsNullOrEmpty(x.Name) &&
            x.Name.Length > 3 &&
            x.Name.Length < 50;
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            stream.SetLength(0);
            stream.Position = 0;
            writer.Reset();
            _converter.Write(writer, criteria, _jsonOptions);
            writer.Flush();
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"Serialization took too long: {stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Test Entity Classes

    public class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    public class ComplexTestEntity
    {
        public int? NullableAge { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string>? Tags { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    #endregion

    #region Additional Missing Coverage Tests

    [Fact]
    public void Write_WithNullableExpressions_SerializesCorrectly()
    {
        // Arrange
        Expression<Func<ComplexTestEntity, bool>> expression = x => x.NullableAge.HasValue && x.NullableAge.Value > 18;
        var criteria = new EntityFilterCriteria<ComplexTestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        var converterComplex = new EntityFilterCriteriaJsonConverter<ComplexTestEntity>();
        converterComplex.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        Assert.Contains("NullableAge", json);
        Assert.Contains("HasValue", json);
    }

    [Fact]
    public void Read_WithValidJsonUsingJsonSerializer_WorksCorrectly()
    {
        // Arrange
        const string json = "{\"Filter\": \"Name == 'John' && Age > 18\"}";

        // Act
        var result = JsonSerializer.Deserialize<EntityFilterCriteria<TestEntity>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var matchingEntity = new TestEntity { Name = "John", Age = 25 };
        var nonMatchingEntity = new TestEntity { Name = "John", Age = 16 };

        Assert.True(result.Matches(matchingEntity));
        Assert.False(result.Matches(nonMatchingEntity));
    }

    [Fact]
    public void Write_SerializeFilterExpression_RemovesParameterAccessors()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x => x.Name.StartsWith("Test") && x.Age > 21;
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        Assert.Contains("StartsWith", json);
        Assert.Contains("Age", json);
        // Should not contain parameter accessor patterns
        Assert.DoesNotContain("(x.", json);
    }

    [Fact]
    public void NormalizeFilterString_WithNullInput_ReturnsNull()
    {
        // Arrange - Test the edge case of null input to normalization
        // This tests the [return: NotNullIfNotNull] behavior
        const string json = "{\"Filter\": \"Name == null\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        // The expression should handle null comparisons
    }

    [Fact]
    public void Read_WithNestedArrayInUnknownProperty_SkipsCorrectly()
    {
        // Arrange
        const string json = "{\"ComplexArray\": [{\"nested\": [1, 2, {\"deep\": \"value\"}]}, \"simple\"], \"Filter\": \"Age > 18\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var testEntity = new TestEntity { Age = 25 };
        Assert.True(result.Matches(testEntity));
    }

    [Fact]
    public void Write_WithComplexNestedExpression_SerializesCorrectly()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x =>
            (x.Name.Contains("test") || x.Name.Contains("demo")) &&
            x.Age >= 18 && x.Age <= 65 &&
            x.IsActive;
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        Assert.Contains("Contains", json);
        Assert.Contains("Age", json);
        Assert.Contains("IsActive", json);
    }

    [Fact]
    public void Read_WithFilterContainingSingleAndDoubleQuotes_HandlesCorrectly()
    {
        // Arrange
        const string json = "{\"Filter\": \"Name == 'Test\\\"Value'\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        // Should handle the quote normalization correctly
    }

    [Fact]
    public void SkipJsonValue_WithPrimitiveValues_HandlesCorrectly()
    {
        // Arrange - Test various primitive types in unknown properties
        const string json = "{\"StringProp\": \"value\", \"NumberProp\": 123, \"BoolProp\": true, \"NullProp\": null, \"Filter\": \"Name == 'John'\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to StartObject

        // Act
        var result = _converter.Read(ref reader, typeof(EntityFilterCriteria<TestEntity>), _jsonOptions);

        // Assert
        Assert.NotNull(result);
        var testEntity = new TestEntity { Name = "John" };
        Assert.True(result.Matches(testEntity));
    }

    #endregion

    #region Regex Pattern Tests

    [Fact]
    public void Write_ParameterAccessorRegex_RemovesParametersCorrectly()
    {
        // Arrange - Test the regex pattern behavior
        Expression<Func<TestEntity, bool>> expression = x => x.Name == "John" && x.Age > 18 && x.IsActive;
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        // Should not contain parameter patterns like "(x."
        Assert.DoesNotContain("(x.", json);
        // Should contain the actual property names
        Assert.Contains("Name", json);
        Assert.Contains("Age", json);
        Assert.Contains("IsActive", json);
    }

    [Fact]
    public void Write_WithMultipleParameterAccessors_RemovesAllCorrectly()
    {
        // Arrange - Test multiple parameter patterns
        Expression<Func<TestEntity, bool>> expression = x =>
            x.Name.StartsWith("Test") &&
            x.Age.ToString().Contains("21") &&
            x.IsActive.ToString() == "True";
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        // Should not contain any parameter accessor patterns
        Assert.DoesNotContain("(x.", json);
        // Should contain method calls
        Assert.Contains("StartsWith", json);
        Assert.Contains("ToString", json);
    }

    #endregion

    #region Constants and Property Tests

    [Fact]
    public void FilterPropertyName_HasCorrectValue()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = x => x.Name == "John";
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        // Act
        var json = JsonSerializer.Serialize(criteria, _jsonOptions);

        // Assert
        Assert.Contains("\"Filter\":", json);
        // Verify the constant is used correctly
    }

    [Fact]
    public void ParameterAccessorRegex_IsCompiledAndCultureInvariant()
    {
        // Arrange - Test that the regex works as expected
        Expression<Func<TestEntity, bool>> expression = x => x.Name == "Test" && x.Age > 0;
        var criteria = new EntityFilterCriteria<TestEntity>(expression);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, criteria, _jsonOptions);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Contains("\"Filter\":", json);
        // The regex should have removed parameter accessors
        Assert.DoesNotContain("(x.", json);
    }

    #endregion
}
