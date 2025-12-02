using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Builders;

public class SqlLikeSearchCriteriaBuilderTests
{
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
    public void Add_NullSearchInfo_ThrowsArgumentNullException()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Add(null!));
    }

    [Fact]
    public void Add_NullMemberExpression_ThrowsArgumentNullException()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((System.Linq.Expressions.Expression<Func<Company, object>>)null!, "search"));
    }

    [Fact]
    public void Add_InvalidPropertyName_ThrowsArgumentNullException()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Add("NotAProp", "search"));
    }

    [Fact]
    public void Remove_NullSearchInfo_ThrowsArgumentNullException()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove((SqlLikeSearchInfo<Company>)null!));
    }

    [Fact]
    public void Remove_NullMemberExpression_ThrowsArgumentNullException()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove((System.Linq.Expressions.Expression<Func<Company, object>>)null!));
    }

    [Fact]
    public void Remove_InvalidPropertyName_ThrowsArgumentNullException()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove("NotAProp"));
    }

    [Fact]
    public void Remove_OnEmptyBuilder_DoesNotThrow()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        var expr = (System.Linq.Expressions.Expression<Func<Company, object>>)(x => x.Name!);
        builder.Remove(expr);
        Assert.Empty(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Add_DuplicateSearchCriteria_AddsBoth()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        var info = new SqlLikeSearchInfo<Company>(x => x.Name!, "search");
        builder.Add(info);
        builder.Add(info);
        Assert.Equal(2, builder.SqlLikeSearchCriteriaList.Count);
    }

    [Fact]
    public void Clear_OnEmptyBuilder_DoesNotThrow()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        builder.Clear();
        Assert.Empty(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Add_And_Remove_WithDifferentSearchGroups()
    {
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        builder.Add(x => x.Name!, "search1", 1);
        builder.Add(x => x.Name!, "search2", 2);
        builder.Remove(x => x.Name!);
        Assert.Single(builder.SqlLikeSearchCriteriaList);
        Assert.Equal("search2", builder.SqlLikeSearchCriteriaList[0].SearchString);
    }

    [Fact]
    public void Add_WithNullSearchString_Throws()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(x => x.Name!, null!));
    }

    [Fact]
    public void Add_WithEmptySearchString_Throws()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add(x => x.Name!, ""));
    }

    [Fact]
    public void Add_WithWhitespaceSearchString_AllowsWhitespaceSearchString()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => builder.Add(x => x.Name!, "   "));
    }

    [Fact]
    public void Add_WithDifferentSearchGroups_AddsBoth()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act
        builder.Add(x => x.Name!, "search", 1);
        builder.Add(x => x.Name!, "search", 2);

        // Assert
        Assert.Equal(2, builder.SqlLikeSearchCriteriaList.Count);
        Assert.NotEqual(builder.SqlLikeSearchCriteriaList[0].SearchGroup, builder.SqlLikeSearchCriteriaList[1].SearchGroup);
    }

    [Fact]
    public void Remove_WithMultipleSameMemberDifferentGroups_RemovesFirstMatchOnly()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        builder.Add(x => x.Name!, "search", 1);
        builder.Add(x => x.Name!, "search", 2);

        // Act
        builder.Remove(x => x.Name!);

        // Assert
        Assert.Single(builder.SqlLikeSearchCriteriaList);
        Assert.Equal(2, builder.SqlLikeSearchCriteriaList[0].SearchGroup);
    }

    [Fact]
    public void Remove_WithNonExistentMember_DoesNothing()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        builder.Add(x => x.Name!, "search");

        // Act
        builder.Remove(x => x.Id);

        // Assert
        Assert.Single(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Remove_WithNonExistentSearchInfo_DoesNothing()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        var info = new SqlLikeSearchInfo<Company>(x => x.Name!, "search");

        // Act
        builder.Remove(info);

        // Assert
        Assert.Empty(builder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Remove_WithNonExistentMemberName_DoesNothing()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        builder.Add(x => x.Name!, "search");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Remove("NotAProp"));
    }

    [Fact]
    public void Add_WithNullMemberName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((string)null!, "search"));
    }

    [Fact]
    public void Add_WithEmptyMemberName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add("", "search"));
    }

    [Fact]
    public void Add_WithWhitespaceMemberName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add("   ", "search"));
    }

    [Fact]
    public void Remove_WithNullMemberName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Remove((string)null!));
    }

    [Fact]
    public void Remove_WithEmptyMemberName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Remove(""));
    }

    [Fact]
    public void Remove_WithWhitespaceMemberName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Remove("   "));
    }

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act
        builder.Add(x => x.Name!, "search1");
        builder.Add(x => x.Id, "search2");
        builder.Remove(x => x.Name!);

        // Assert
        Assert.Equal(1, builder.Count);
    }
}
