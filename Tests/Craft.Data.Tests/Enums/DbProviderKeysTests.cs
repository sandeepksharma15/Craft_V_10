namespace Craft.Data.Tests.Enums;

public class DbProviderKeysTests
{
    [Fact]
    public void MySql_Should_Have_Correct_Value()
    {
        // Assert
        Assert.Equal("mysql", DbProviderKeys.MySql);
    }

    [Fact]
    public void Npgsql_Should_Have_Correct_Value()
    {
        // Assert
        Assert.Equal("postgresql", DbProviderKeys.Npgsql);
    }

    [Fact]
    public void SqlServer_Should_Have_Correct_Value()
    {
        // Assert
        Assert.Equal("mssql", DbProviderKeys.SqlServer);
    }

    [Theory]
    [InlineData("mysql")]
    [InlineData("postgresql")]
    [InlineData("mssql")]
    public void All_Keys_Should_Be_Lowercase(string key)
    {
        // Assert
        Assert.Equal(key.ToLowerInvariant(), key);
    }
}
