using System;
using Craft.Utilities.Builders;
using Xunit;

namespace Craft.Utilities.Tests.Builders;

public class ValueBuilderTests
{
    [Fact]
    public void Default_HasNoValue()
    {
        // Arrange & Act
        var builder = new ValueBuilder();

        // Assert
        Assert.False(builder.HasValue);
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddValue_String_WhenTrue_AppendsValue()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue("foo", true);

        // Assert
        Assert.True(builder.HasValue);
        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void AddValue_String_WhenFalse_DoesNotAppend()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue("foo", false);

        // Assert
        Assert.False(builder.HasValue);
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddValue_Func_WhenTrue_AppendsValue()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue(() => "bar", true);

        // Assert
        Assert.True(builder.HasValue);
        Assert.Equal("bar", builder.ToString());
    }

    [Fact]
    public void AddValue_Func_WhenFalse_DoesNotAppend()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue(() => "bar", false);

        // Assert
        Assert.False(builder.HasValue);
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddValue_Chaining_AppendsMultipleValues()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue("foo").AddValue("bar");

        // Assert
        Assert.True(builder.HasValue);
        Assert.Equal("foo bar", builder.ToString());
    }

    [Fact]
    public void AddValue_MixedChaining_AppendsAllValues()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue("foo").AddValue(() => "bar").AddValue("baz", false);

        // Assert
        Assert.True(builder.HasValue);
        Assert.Equal("foo bar", builder.ToString());
    }

    [Fact]
    public void ToString_TrimmedResult()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue("  foo  ");

        // Assert
        Assert.Equal("foo", builder.ToString());
    }

    [Fact]
    public void AddValue_EmptyString_DoesNotAffectHasValue()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue("");

        // Assert
        Assert.False(builder.HasValue);
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddValue_NullString_DoesNotThrowOrAffectHasValue()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue((string)null!);

        // Assert
        Assert.False(builder.HasValue);
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void AddValue_FuncReturnsNull_DoesNotThrowOrAffectHasValue()
    {
        // Arrange
        var builder = new ValueBuilder();

        // Act
        builder.AddValue(() => null!);

        // Assert
        Assert.False(builder.HasValue);
        Assert.Equal(string.Empty, builder.ToString());
    }
}
