using System.Globalization;

namespace Craft.Expressions.Tests;

public class CultureInvariantTests
{
    private static ExpressionSerializer<TestClass> Serializer => new();

    private class TestClass
    {
        public double Price { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    [Fact]
    public void Deserialize_ParsesDecimalWithPeriod_InAllCultures()
    {
        // Arrange
        var currentCulture = CultureInfo.CurrentCulture;

        try
        {
            // Test with different cultures
            var cultures = new[]
            {
                CultureInfo.GetCultureInfo("en-US"), // Period as decimal separator
                CultureInfo.GetCultureInfo("de-DE"), // Comma as decimal separator
                CultureInfo.GetCultureInfo("fr-FR")  // Comma as decimal separator
            };

            foreach (var culture in cultures)
            {
                CultureInfo.CurrentCulture = culture;

                // Act
                var expression = Serializer.Deserialize("Price > 10.50");
                var compiled = expression.Compile();

                // Assert
                Assert.True(compiled(new TestClass { Price = 11.0 }));
                Assert.False(compiled(new TestClass { Price = 10.0 }));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    [Fact]
    public void Deserialize_ParsesIntegerConsistently_InAllCultures()
    {
        // Arrange
        var currentCulture = CultureInfo.CurrentCulture;

        try
        {
            var cultures = new[]
            {
                CultureInfo.GetCultureInfo("en-US"),
                CultureInfo.GetCultureInfo("de-DE"),
                CultureInfo.GetCultureInfo("ja-JP")
            };

            foreach (var culture in cultures)
            {
                CultureInfo.CurrentCulture = culture;

                // Act - use Count (int) to compare with int
                var expression = Serializer.Deserialize("Count == 42");
                var compiled = expression.Compile();

                // Assert
                Assert.True(compiled(new TestClass { Count = 42 }));
                Assert.False(compiled(new TestClass { Count = 43 }));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    [Fact]
    public void Deserialize_ParsesDoubleConsistently_InAllCultures()
    {
        // Arrange
        var currentCulture = CultureInfo.CurrentCulture;

        try
        {
            var cultures = new[]
            {
                CultureInfo.GetCultureInfo("en-US"),
                CultureInfo.GetCultureInfo("de-DE"),
                CultureInfo.GetCultureInfo("ar-SA")
            };

            foreach (var culture in cultures)
            {
                CultureInfo.CurrentCulture = culture;

                // Act
                var expression = Serializer.Deserialize("Price < 5.5");
                var compiled = expression.Compile();

                // Assert
                Assert.True(compiled(new TestClass { Price = 4.0 }));
                Assert.False(compiled(new TestClass { Price = 6.0 }));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }
}

