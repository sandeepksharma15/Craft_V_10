using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Core;

public class QueryPaginationExtensionTests
{
    [Fact]
    public void Skip_WhenQueryIsNotNull_SetsSkipCorrectly()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        const int skipValue = 10;

        // Act
        var result = query.Skip(skipValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(skipValue, query.Skip);
    }

    [Fact]
    public void Skip_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.Skip(5);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Take_WhenQueryIsNotNull_SetsTakeCorrectly()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        const int takeValue = 10;

        // Act
        var result = query.Take(takeValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(takeValue, query.Take);
    }

    [Fact]
    public void Take_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.Take(5);

        // Assert
        Assert.Null(result);
    }
}
