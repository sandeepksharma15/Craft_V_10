using System.Text.Json;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Builders;

public class SqlSearchCriteriaBuilderTests
{
    private readonly JsonSerializerOptions serializeOptions;

    public SqlSearchCriteriaBuilderTests()
    {
        serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new SqlSearchCriteriaBuilderJsonConverter<Company>());
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldInitializeSearchCriteriaList()
    {
        // Arrange & Act
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Assert
        Assert.NotNull(builder.SqlLikeSearchCriteriaList);
        Assert.Empty(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Add_WithSearchInfo_ShouldAddToSearchCriteriaList()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        var searchInfo = new SqlLikeSearchInfo<Company>(x => x.Name!, "searchString");

        // Act
        builder.Add(searchInfo);

        // Assert
        Assert.Contains(searchInfo, builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Add_WithMemberAndSearchTerm_ShouldAddToSearchCriteriaList()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act
        builder.Add(x => x.Name!, "searchString");

        // Assert
        Assert.Single(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Add_WithMemberNameAndSearchTerm_ShouldAddToSearchCriteriaList()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act
        builder.Add("Name", "searchString");

        // Assert
        Assert.Single(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Clear_WhenCalled_ShouldClearSearchCriteriaList()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        builder.Add(x => x.Name!, "searchString");

        // Act
        builder.Clear();

        // Assert
        Assert.Empty(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Remove_WithSearchInfo_ShouldRemoveFromSearchCriteriaList()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        var searchInfo = new SqlLikeSearchInfo<Company>(x => x.Name!, "searchString");
        builder.Add(searchInfo);

        // Act
        builder.Remove(searchInfo);

        // Assert
        Assert.DoesNotContain(searchInfo, builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Remove_WithMemberExpression_ShouldRemoveFromSearchCriteriaList()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        builder.Add(x => x.Name!, "searchString");

        // Act
        builder.Remove(x => x.Name!);

        // Assert
        Assert.Empty(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Remove_WithMemberName_ShouldRemoveFromSearchCriteriaList()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        builder.Add("Name", "searchString");

        // Act
        builder.Remove("Name");

        // Assert
        Assert.Empty(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void CanConvert_ReturnsTrueForSqlSearchCriteriaBuilderType()
    {
        var converter = new SqlSearchCriteriaBuilderJsonConverter<TestClass>();
        bool canConvert = converter.CanConvert(typeof(SqlLikeSearchCriteriaBuilder<TestClass>));
        Assert.True(canConvert);
    }

    [Fact]
    public void Read_DeserializesValidJsonToSqlSearchCriteriaBuilder()
    {
        // Arrange
        const string json = "[{\"SearchItem\": \"Name\", \"SearchString\": \"John\", \"SearchGroup\": 1}]";

        // Act
        var searchBuilder = JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<Company>>(json, serializeOptions);

        // Assert
        Assert.Equal(1, searchBuilder?.Count);
        Assert.Contains("x.Name", searchBuilder?.SqlLikeSearchCriteriaList[0]?.SearchItem?.Body.ToString());
        Assert.Equal("John", searchBuilder?.SqlLikeSearchCriteriaList[0]?.SearchString);
        Assert.Equal(1, searchBuilder?.SqlLikeSearchCriteriaList[0]?.SearchGroup);
    }

    [Fact]
    public void Read_ThrowsJsonExceptionForInvalidJsonFormat()
    {
        // Arrange
        const string json = "{}"; // Not an array

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<Company>>(json, serializeOptions));
    }

    [Fact]
    public void Write_SerializesSqlSearchCriteriaBuilderToJsonCorrectly()
    {
        // Arrange
        var searchBuilder = new SqlLikeSearchCriteriaBuilder<Company>();
        searchBuilder.Add(new SqlLikeSearchInfo<Company>(x => x.Name!, "Alice", 2));

        // Act
        var json = JsonSerializer.Serialize(searchBuilder, serializeOptions);

        // Assert
        Assert.Equal("[{\"SearchItem\":\"Name\",\"SearchString\":\"Alice\",\"SearchGroup\":2}]", json);
    }
}
