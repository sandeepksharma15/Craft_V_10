using System.Text.RegularExpressions;

namespace Craft.Domain.Tests.Base;

public class DomainObjectTests
{
    #region Test Implementations

    private sealed class TestDomainObject : DomainObject
    {
        public static void InvokeEnsure(bool condition, string message)
            => Ensure(condition, message);

        public static string InvokeNotEmpty(string? value, string paramName)
            => NotEmpty(value, paramName);

        public static T InvokeNotNull<T>(T? value, string paramName) where T : class
            => NotNull(value, paramName);
    }

    #endregion

    [Fact]
    public void Ensure_Should_NotThrow_WhenConditionIsTrue()
    {
        // Arrange
        var sut = new TestDomainObject();

        // Act
        var exception = Record.Exception(() => TestDomainObject.InvokeEnsure(condition: true, "failure"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Ensure_Should_ThrowArgumentException_WhenConditionIsFalse()
    {
        // Arrange
        var sut = new TestDomainObject();

        // Act
        var exception = Assert.Throws<ArgumentException>(() => TestDomainObject.InvokeEnsure(condition: false, "failure"));

        // Assert
        Assert.Equal("failure", exception.Message);
    }

    [Fact]
    public void NotEmpty_Should_ReturnTrimmedValue_WhenValueHasWhitespace()
    {
        // Arrange
        // Act
        var result = TestDomainObject.InvokeNotEmpty("  value  ", "value");

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void NotEmpty_Should_ThrowArgumentException_WhenValueIsWhitespace()
    {
        // Arrange
        var sut = new TestDomainObject();

        // Act
        var exception = Assert.Throws<ArgumentException>(() => TestDomainObject.InvokeNotEmpty("   ", "value"));

        // Assert
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void NotNull_Should_ReturnReference_WhenValueIsNotNull()
    {
        // Arrange
        var value = new object();

        // Act
        var result = TestDomainObject.InvokeNotNull(value, nameof(value));

        // Assert
        Assert.Same(value, result);
    }

    [Fact]
    public void NotNull_Should_ThrowArgumentNullException_WhenValueIsNull()
    {
        // Arrange
        var sut = new TestDomainObject();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => TestDomainObject.InvokeNotNull<object>(null, "value"));

        // Assert
        Assert.Equal("value", exception.ParamName);
    }
}

public class NumericValueObjectTests
{
    #region Test Implementations

    private sealed class PositiveCount : NumericValueObject<int, PositiveCount>
    {
        public PositiveCount(int value) : base(value) { }

        protected override bool IsValid(int value)
            => value > 0;

        protected override string ValidationMessage
            => "Value must be greater than zero.";
    }

    #endregion

    [Fact]
    public void Constructor_Should_SetValue_WhenValueIsValid()
    {
        // Arrange & Act
        var valueObject = new PositiveCount(5);

        // Assert
        Assert.Equal(5, valueObject.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Should_ThrowArgumentException_WhenValueIsInvalid(int value)
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(() => new PositiveCount(value));

        // Assert
        Assert.Equal("Value must be greater than zero.", exception.Message);
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnUnderlyingValue()
    {
        // Arrange
        var valueObject = new PositiveCount(7);

        // Act
        int result = valueObject;

        // Assert
        Assert.Equal(7, result);
    }

    [Fact]
    public void ToString_Should_ReturnNumericString()
    {
        // Arrange
        var valueObject = new PositiveCount(42);

        // Act
        var result = valueObject.ToString();

        // Assert
        Assert.Equal("42", result);
    }
}

public class StringValueObjectBaseTests
{
    #region Test Implementations

    private sealed class OrderCode : StringValueObject<OrderCode>, IStringValueObject<OrderCode>
    {
        private static readonly Regex OrderCodeRegex = new("^[A-Z]{2}-\\d{3}$", RegexOptions.Compiled);

        public OrderCode(string value) : base(Validate(value)) { }

        protected override Regex ValidationExpression
            => OrderCodeRegex;

        protected override string ValidationMessage
            => "Order code must match the format AA-999.";

        protected override string Normalize(string value)
            => value.Trim().ToUpperInvariant();

        public static OrderCode Create(string value)
            => new(value);

        public string InvokeNormalizeAndValidate(string value)
            => NormalizeAndValidate(value);

        private static string Validate(string value)
        {
            value = NotEmpty(value, nameof(value));

            string normalized = value.Trim().ToUpperInvariant();
            Ensure(OrderCodeRegex.IsMatch(normalized), "Order code must match the format AA-999.");
            return normalized;
        }
    }

    #endregion

    [Fact]
    public void Parse_Should_CreateInstance_WhenValueIsValid()
    {
        // Arrange & Act
        var result = OrderCode.Parse(" ab-123 ", provider: null);

        // Assert
        Assert.Equal("AB-123", result.Value);
    }

    [Fact]
    public void Parse_Should_ThrowFormatException_WhenValueIsWhitespace()
    {
        // Act
        var exception = Assert.Throws<FormatException>(() => OrderCode.Parse("   ", provider: null));

        // Assert
        Assert.Equal("OrderCode cannot be null or empty.", exception.Message);
    }

    [Fact]
    public void Parse_Should_ThrowFormatException_WhenValueIsInvalid()
    {
        // Act
        var exception = Assert.Throws<FormatException>(() => OrderCode.Parse("invalid", provider: null));

        // Assert
        Assert.Equal("Invalid OrderCode.", exception.Message);
        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    [Fact]
    public void TryParse_Should_ReturnTrue_WhenValueIsValid()
    {
        // Act
        var parsed = OrderCode.TryParse("cd-456", provider: null, out var result);

        // Assert
        Assert.True(parsed);
        Assert.Equal("CD-456", result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    public void TryParse_Should_ReturnFalse_WhenValueIsInvalid(string? value)
    {
        // Act
        var parsed = OrderCode.TryParse(value, provider: null, out var result);

        // Assert
        Assert.False(parsed);
        Assert.Null((object?)result);
    }

    [Fact]
    public void NormalizeAndValidate_Should_ReturnNormalizedValue_WhenValueIsValid()
    {
        // Arrange
        var sut = new OrderCode("AB-123");

        // Act
        var result = sut.InvokeNormalizeAndValidate(" cd-456 ");

        // Assert
        Assert.Equal("CD-456", result);
    }

    [Fact]
    public void NormalizeAndValidate_Should_ThrowArgumentException_WhenValueIsInvalid()
    {
        // Arrange
        var sut = new OrderCode("AB-123");

        // Act
        var exception = Assert.Throws<ArgumentException>(() => sut.InvokeNormalizeAndValidate("invalid"));

        // Assert
        Assert.Equal("Order code must match the format AA-999.", exception.Message);
    }

    [Fact]
    public void ImplicitConversion_Should_ReturnUnderlyingString()
    {
        // Arrange
        var valueObject = OrderCode.Create("ef-789");

        // Act
        string result = valueObject;

        // Assert
        Assert.Equal("EF-789", result);
    }

    [Fact]
    public void ToString_Should_ReturnUnderlyingValue()
    {
        // Arrange
        var valueObject = OrderCode.Create("gh-123");

        // Act
        var result = valueObject.ToString();

        // Assert
        Assert.Equal("GH-123", result);
    }
}

public class AggregateRootBaseTests
{
    #region Test Implementations

    private sealed class GenericAggregateRoot : AggregateRoot<int>
    {
        public GenericAggregateRoot() { }

        public GenericAggregateRoot(int id) : base(id) { }
    }

    private sealed class DefaultAggregateRoot : AggregateRoot
    {
        public DefaultAggregateRoot() { }

        public DefaultAggregateRoot(KeyType id) : base(id) { }
    }

    #endregion

    [Fact]
    public void Constructor_Should_SetId_ForGenericAggregateRoot()
    {
        // Arrange & Act
        var aggregateRoot = new GenericAggregateRoot(17);

        // Assert
        Assert.Equal(17, aggregateRoot.Id);
    }

    [Fact]
    public void Constructor_Should_SetId_ForDefaultAggregateRoot()
    {
        // Arrange & Act
        var aggregateRoot = new DefaultAggregateRoot(23);

        // Assert
        Assert.Equal(23, aggregateRoot.Id);
    }
}
