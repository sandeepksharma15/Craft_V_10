using System.ComponentModel;

namespace Craft.Extensions.Tests.System;

[Flags]
public enum TestEnum
{
    [Description("First description")]
    First = 0,

    [Description("Second description")]
    Second = 1,

    Third = 2
}

[Flags]
public enum SomeEnum
{
    Apple = 0,
    Orange = 1,
    Banana = 2
}

public class EnumTests
{
    public static IEnumerable<object[]> EnumValuesTestData =>
       [
                [TestEnum.First, "First", "First description"],
                [TestEnum.Second, "Second", "Second description"],
                [TestEnum.Third, "Third", "Third"],
       ];

    [Fact]
    public void GetDescription_ShouldReturnDescriptionAttribute_WhenEnumHasOne()
    {
        // Arrange
        const TestEnum someEnum = TestEnum.First;

        // Act
        string description = someEnum.GetDescription();

        // Assert
        Assert.Equal("First description", description);
    }

    [Fact]
    public void GetDescription_ShouldReturnEnumToString_WhenEnumHasNoDescriptionAttribute()
    {
        // Arrange
        const TestEnum someEnum = TestEnum.Third;

        // Act
        string description = someEnum.GetDescription();

        // Assert
        Assert.Equal("Third", description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ToEnum_ValuesExist_ShouldReturnCorrectValue(int value)
    {
        // Arrange
        TestEnum expected = (TestEnum)value;

        // Act
        TestEnum actual = value.ToEnum<TestEnum>();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToEnum_ShouldThrowException_ForNonEnumType()
    {
        // Arrange
        const int value = 4;

        // Act & Assert
        Assert.Throws<Exception>(() => value.ToEnum<int>());
    }

    [Theory]
    [InlineData("Apple", SomeEnum.Apple, true)]
    [InlineData("Orange", SomeEnum.Apple | SomeEnum.Orange, true)]
    [InlineData("Banana", SomeEnum.Apple | SomeEnum.Orange, false)]
    [InlineData("orange", SomeEnum.Orange, true)]
    [InlineData("Orange", (SomeEnum)3, true)]
    public void Contains_ShouldReturnExpectedResult_WhenCalledWithValidInputs(string agent, SomeEnum flags, bool expected)
    {
        // Act
        var actual = agent.Contains(flags);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToStringInvariant_Should_ReturnEnumName()
    {
        // Arrange
        const TestEnum value = TestEnum.Second;

        // Act
        var result = value.ToStringInvariant();

        // Assert
        Assert.Equal("Second", result);
    }

    [Fact]
    public void GetFlags_Should_ReturnEnumFlags()
    {
        // Arrange
        const TestEnum value = TestEnum.First | TestEnum.Second;

        // Act
        var result = value.GetFlags();

        // Assert
        Assert.Contains(TestEnum.First, result);
        Assert.Contains(TestEnum.Second, result);
    }

    [Fact]
    public void IsSet_Should_ReturnTrue_WhenEnumContainsMatchingFlag()
    {
        // Arrange
        const TestEnum input = TestEnum.First | TestEnum.Second;
        const TestEnum matchTo = TestEnum.Second;

        // Act
        var result = input.IsSet(matchTo);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("First", true, TestEnum.First)]
    [InlineData("first", true, TestEnum.First)]
    [InlineData("Second", true, TestEnum.Second)]
    [InlineData("SECOND", true, TestEnum.Second)]
    [InlineData("Third", true, TestEnum.Third)]
    public void ToEnum_ShouldConvertStringToEnum(string value, bool ignoreCase, TestEnum expectedEnum)
    {
        // Act
        var actual = value.ToEnum<TestEnum>(ignoreCase);

        // Assert
        Assert.Equal(expectedEnum, actual);
    }

    [Theory]
    [MemberData(nameof(EnumValuesTestData))]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
    public void GetName_ReturnsCorrectName(TestEnum value, string expectedName, string _)
    {
        // Act
        string actualName = EnumValues<TestEnum>.GetName(value);

        // Assert
        Assert.Equal(expectedName, actualName);
    }

    [Theory]
    [MemberData(nameof(EnumValuesTestData))]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
    public void GetDescription_ReturnsCorrectDescription(TestEnum value, string _, string expectedDescription)
    {
        // Act
        string actualDescription = EnumValues<TestEnum>.GetDescription(value);

        // Assert
        Assert.Equal(expectedDescription, actualDescription);
    }

    [Fact]
    public void GetNames_ReturnsAllNames()
    {
        // Arrange
        var expectedNames = new Dictionary<TestEnum, string>
        {
            { TestEnum.First, "First" },
            { TestEnum.Second, "Second" },
            { TestEnum.Third, "Third" },
        };

        // Act
        var actualNames = EnumValues<TestEnum>.GetNames();

        // Assert
        Assert.Equal(expectedNames, actualNames);
    }

    [Fact]
    public void GetDescriptions_ReturnsAllDescriptions()
    {
        // Arrange
        var expectedDescriptions = new Dictionary<TestEnum, string>
        {
            { TestEnum.First, "First description" },
            { TestEnum.Second, "Second description" },
            { TestEnum.Third, "Third" },
        };

        // Act
        var actualDescriptions = EnumValues<TestEnum>.GetDescriptions();

        // Assert
        Assert.Equal(expectedDescriptions, actualDescriptions);
    }

    [Fact]
    public void GetValues_ReturnsAllValues()
    {
        // Arrange
        var expectedValues = new[] { TestEnum.First, TestEnum.Second, TestEnum.Third };

        // Act
        var actualValues = EnumValues<TestEnum>.GetValues();

        // Assert
        Assert.Equal(expectedValues, actualValues);
    }

    [Theory]
    [MemberData(nameof(EnumValuesTestData))]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
    public void TryGetSingleName_ReturnsTrueAndCorrectName(TestEnum value, string expectedName, string _)
    {
        // Act
        bool result = EnumValues<TestEnum>.TryGetSingleName(value, out string? actualName);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedName, actualName);
    }

    [Theory]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "<Pending>")]
    [MemberData(nameof(EnumValuesTestData))]
    public void TryGetSingleDescription_ReturnsTrueAndCorrectDescription(TestEnum value, string _, string expectedDescription)
    {
        // Act
        bool result = EnumValues<TestEnum>.TryGetSingleDescription(value, out string? actualDescription);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedDescription, actualDescription);
    }
}
