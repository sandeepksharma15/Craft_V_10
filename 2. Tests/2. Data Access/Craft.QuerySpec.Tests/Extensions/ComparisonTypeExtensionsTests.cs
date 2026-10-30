namespace Craft.QuerySpec.Tests.Extensions;

public class ComparisonTypeExtensionsTests
{
    [Fact]
    public void GetValidComparisonOperators_StringBackedValueObject_ReturnsContainsOnly()
    {
        var operators = typeof(TestCode).GetValidComparisonOperators();

        var comparison = Assert.Single(operators);
        Assert.Equal(ComparisonType.Contains, comparison);
    }

    [Fact]
    public void GetValidComparisonOperators_String_ReturnsStandardStringOperators()
    {
        var operators = typeof(string).GetValidComparisonOperators();

        Assert.Equal(
            [
                ComparisonType.EqualTo,
                ComparisonType.NotEqualTo,
                ComparisonType.Contains,
                ComparisonType.StartsWith,
                ComparisonType.EndsWith
            ],
            operators);
    }

    private readonly record struct TestCode(string Value);
}
