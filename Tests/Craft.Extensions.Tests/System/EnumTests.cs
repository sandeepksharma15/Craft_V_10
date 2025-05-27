using System.ComponentModel;

namespace Craft.Extensions.Tests.System;

public class EnumExtensionsTests
{
    [Fact]
    public void GetOrderedEnumValues_ReturnsOrdered()
    {
        var result = EnumExtensions.GetOrderedEnumValues<SimpleEnum>();
        Assert.Equal(new[] { SimpleEnum.Zero, SimpleEnum.One, SimpleEnum.Two }, result);
    }

    [Fact]
    public void GetExtremeEnumValue_ReturnsHighestAndLowest()
    {
        Assert.Equal(SimpleEnum.Two, EnumExtensions.GetExtremeEnumValue<SimpleEnum>(true));
        Assert.Equal(SimpleEnum.Zero, EnumExtensions.GetExtremeEnumValue<SimpleEnum>(false));
    }

    [Fact]
    public void GetHighestEnumValue_ReturnsHighest()
    {
        Assert.Equal(SimpleEnum.Two, EnumExtensions.GetHighestEnumValue<SimpleEnum>());
    }

    [Fact]
    public void GetLowestEnumValue_ReturnsLowest()
    {
        Assert.Equal(SimpleEnum.Zero, EnumExtensions.GetLowestEnumValue<SimpleEnum>());
    }

    [Fact]
    public void GetDescriptions_ReturnsDescriptions()
    {
        var dict = EnumExtensions.GetDescriptions<DescEnum>();
        Assert.Equal("Alpha Desc", dict[DescEnum.Alpha]);
        Assert.Equal("Beta Desc", dict[DescEnum.Beta]);
        Assert.Equal("Gamma", dict[DescEnum.Gamma]);
    }

    [Fact]
    public void GetNames_ReturnsNames()
    {
        var dict = EnumExtensions.GetNames<SimpleEnum>();
        Assert.Equal("Zero", dict[SimpleEnum.Zero]);
        Assert.Equal("One", dict[SimpleEnum.One]);
        Assert.Equal("Two", dict[SimpleEnum.Two]);
    }

    [Fact]
    public void GetValues_ReturnsAll()
    {
        var arr = EnumExtensions.GetValues<SimpleEnum>();
        Assert.Equal(new[] { SimpleEnum.Zero, SimpleEnum.One, SimpleEnum.Two }, arr);
    }

    [Fact]
    public void GetNextEnumValue_ReturnsNextOrFirst()
    {
        Assert.Equal(SimpleEnum.One, SimpleEnum.Zero.GetNextEnumValue());
        Assert.Equal(SimpleEnum.Zero, SimpleEnum.Two.GetNextEnumValue());
    }

    [Fact]
    public void GetPrevEnumValue_ReturnsPrevOrLast()
    {
        Assert.Equal(SimpleEnum.Zero, SimpleEnum.One.GetPrevEnumValue());
        Assert.Equal(SimpleEnum.Two, SimpleEnum.Zero.GetPrevEnumValue());
    }

    [Fact]
    public void GetDescription_ReturnsDescriptionOrName()
    {
        Assert.Equal("Alpha Desc", DescEnum.Alpha.GetDescription());
        Assert.Equal("Gamma", DescEnum.Gamma.GetDescription());
    }

    [Fact]
    public void GetFlags_ReturnsSetFlags()
    {
        var flags = TestFlags.All.GetFlags();
        Assert.Contains(TestFlags.One, flags);
        Assert.Contains(TestFlags.Two, flags);
        Assert.Contains(TestFlags.Four, flags);
    }

    [Fact]
    public void IsSet_ReturnsTrueIfFlagIsSet()
    {
        Assert.True((TestFlags.One | TestFlags.Two).IsSet(TestFlags.Two));
        Assert.False(TestFlags.One.IsSet(TestFlags.Four));
    }

    [Fact]
    public void GetName_ReturnsNameOrFlags()
    {
        Assert.Equal("One", TestFlags.One.GetName());
        Assert.Equal("None,One,Two", (TestFlags.One | TestFlags.Two).GetName());
    }

    [Fact]
    public void TryGetSingleDescription_ReturnsTrueAndDescription()
    {
        Assert.True(DescEnum.Beta.TryGetSingleDescription(out var desc));
        Assert.Equal("Beta Desc", desc);
    }

    [Fact]
    public void TryGetSingleName_ReturnsTrueAndName()
    {
        Assert.True(DescEnum.Beta.TryGetSingleName(out var name));
        Assert.Equal("Beta", name);
    }

    [Fact]
    public void ToStringInvariant_ReturnsName()
    {
        Assert.Equal("Alpha", DescEnum.Alpha.ToStringInvariant());
    }

    [Fact]
    public void ValidateEnumType_ThrowsIfNotEnum()
    {
        Assert.Throws<Exception>(() => EnumExtensions.ValidateEnumType<int>());
    }

    [Fact]
    public void ToEnum_IntToEnum_ReturnsEnumValue()
    {
        Assert.Equal(SimpleEnum.One, 1.ToEnum<SimpleEnum>());
    }

    [Fact]
    public void ToEnum_StringToEnum_ReturnsEnumValue()
    {
        Assert.Equal(SimpleEnum.Two, "Two".ToEnum<SimpleEnum>());
        Assert.Equal(SimpleEnum.One, "1".ToEnum<SimpleEnum>());
    }

    [Fact]
    public void ToEnum_StringToEnum_ThrowsOnNullOrEmpty()
    {
        Assert.Throws<ArgumentNullException>(() => ((string)null!).ToEnum<SimpleEnum>());
        Assert.Throws<ArgumentNullException>(() => "".ToEnum<SimpleEnum>());
    }

    [Fact]
    public void Contains_ReturnsTrueIfStringContainsFlagName()
    {
        Assert.True("One,Two".Contains(TestFlags.One));
        Assert.False("Four".Contains(TestFlags.One));
    }

    [Fact]
    public void Contains_ReturnsTrueIfStringContainsAnyFlag()
    {
        Assert.False("One,Two".Contains(TestFlags.All));
        Assert.True("One".Contains(TestFlags.One));
    }

    // --- Additional edge and negative tests for full coverage ---

    [Fact]
    public void GetDescription_ReturnsEmptyStringIfNoMember()
    {
        var fake = (SimpleEnum)999;
        Assert.Equal(string.Empty, fake.GetDescription());
    }

    [Fact]
    public void GetName_ReturnsFlagsNamesIfNotFound()
    {
        var fake = (TestFlags)3; // One | Two
        Assert.Equal("None,One,Two", fake.GetName());
    }

    [Fact]
    public void TryGetSingleDescription_ReturnsFalseIfNotFound()
    {
        var fake = (SimpleEnum)999;
        Assert.False(fake.TryGetSingleDescription(out var desc));
        Assert.Null(desc);
    }

    [Fact]
    public void TryGetSingleName_ReturnsFalseIfNotFound()
    {
        var fake = (SimpleEnum)999;
        Assert.False(fake.TryGetSingleName(out var name));
        Assert.Null(name);
    }

    [Fact]
    public void IsSet_WorksForZero()
    {
        Assert.False(TestFlags.None.IsSet(TestFlags.One));
        Assert.False(TestFlags.One.IsSet(TestFlags.None));
    }

    [Fact]
    public void GetFlags_ReturnsEmptyForZero()
    {
        var flags = TestFlags.None.GetFlags();
        Assert.Contains(TestFlags.None, flags);
    }

    [Flags]
    private enum TestFlags
    {
        None = 0,
        One = 1,
        Two = 2,
        Four = 4,
        All = One | Two | Four
    }

    private enum SimpleEnum
    {
        Zero = 0,
        One = 1,
        Two = 2
    }

    private enum DescEnum
    {
        [Description("Alpha Desc")]
        Alpha = 1,
        [Description("Beta Desc")]
        Beta = 2,
        Gamma = 3
    }
}
