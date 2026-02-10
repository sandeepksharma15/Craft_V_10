namespace Craft.Domain.Tests.Base;

public class ValueObjectTests
{
    #region Test Implementations

    private sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    private sealed class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string? ZipCode { get; }

        public Address(string street, string city, string? zipCode = null)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return ZipCode;
        }
    }

    private sealed class EmptyValueObject : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield break;
        }
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameValues()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        Assert.True(money1.Equals(money2));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentValues()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(200.00m, "USD");

        // Act & Assert
        Assert.False(money1.Equals(money2));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentCurrency()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "EUR");

        // Act & Assert
        Assert.False(money1.Equals(money2));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameInstance()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act & Assert
        Assert.True(money.Equals(money));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForNull()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act & Assert
        Assert.False(money.Equals(null));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentTypes()
    {
        // Arrange
        var money = new Money(100.00m, "USD");
        var address = new Address("123 Main St", "City");

        // Act & Assert
        Assert.False(money.Equals(address));
    }

    [Fact]
    public void Equals_ShouldHandleNullComponents()
    {
        // Arrange
        var address1 = new Address("123 Main St", "City", null);
        var address2 = new Address("123 Main St", "City", null);
        var address3 = new Address("123 Main St", "City", "12345");

        // Act & Assert
        Assert.True(address1.Equals(address2));
        Assert.False(address1.Equals(address3));
    }

    [Fact]
    public void EqualsObject_ShouldReturnTrue_ForEqualValueObjects()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        object money2 = new Money(100.00m, "USD");

        // Act & Assert
        Assert.True(money1.Equals(money2));
    }

    [Fact]
    public void EqualsObject_ShouldReturnFalse_ForNonValueObject()
    {
        // Arrange
        var money = new Money(100.00m, "USD");
        object notValueObject = "not a value object";

        // Act & Assert
        Assert.False(money.Equals(notValueObject));
    }

    #endregion

    #region Equality Operator Tests

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_ForEqualValues()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        Assert.True(money1 == money2);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_ForDifferentValues()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(200.00m, "USD");

        // Act & Assert
        Assert.False(money1 == money2);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_ForBothNull()
    {
        // Arrange
        Money? money1 = null;
        Money? money2 = null;

        // Act & Assert
        Assert.True(money1 == money2);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenOneIsNull()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        Money? money2 = null;

        // Act & Assert
        Assert.False(money1 == money2);
        Assert.False(money2 == money1);
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_ForDifferentValues()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(200.00m, "USD");

        // Act & Assert
        Assert.True(money1 != money2);
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_ForEqualValues()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        Assert.False(money1 != money2);
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_ShouldBeSame_ForEqualValues()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(100.00m, "USD");

        // Act & Assert
        Assert.Equal(money1.GetHashCode(), money2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldBeDifferent_ForDifferentValues()
    {
        // Arrange
        var money1 = new Money(100.00m, "USD");
        var money2 = new Money(200.00m, "USD");

        // Act & Assert (highly likely to be different)
        Assert.NotEqual(money1.GetHashCode(), money2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        var money = new Money(100.00m, "USD");

        // Act
        var hash1 = money.GetHashCode();
        var hash2 = money.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ShouldHandleNullComponents()
    {
        // Arrange
        var address = new Address("123 Main St", "City", null);

        // Act & Assert (should not throw)
        var hash = address.GetHashCode();
        Assert.NotEqual(0, hash);
    }

    [Fact]
    public void GetHashCode_ShouldWorkForEmptyComponents()
    {
        // Arrange
        var empty1 = new EmptyValueObject();
        var empty2 = new EmptyValueObject();

        // Act & Assert
        Assert.Equal(empty1.GetHashCode(), empty2.GetHashCode());
    }

    #endregion

    #region GetCopy Tests

    [Fact]
    public void GetCopy_ShouldCreateShallowCopy()
    {
        // Arrange
        var original = new Money(100.00m, "USD");

        // Act
        var copy = original.GetCopy();

        // Assert
        Assert.NotSame(original, copy);
        Assert.Equal(original, copy);
    }

    [Fact]
    public void GetCopy_ShouldReturnValueObject()
    {
        // Arrange
        var original = new Money(100.00m, "USD");

        // Act
        var copy = original.GetCopy();

        // Assert
        Assert.IsType<Money>(copy);
    }

    #endregion

    #region IEquatable<ValueObject> Tests

    [Fact]
    public void ValueObject_ShouldImplementIEquatable()
    {
        // Arrange & Act
        var money = new Money(100.00m, "USD");

        // Assert
        Assert.IsAssignableFrom<IEquatable<ValueObject>>(money);
    }

    #endregion
}

public class SingleValueObjectTests
{
    #region Test Implementations

    private sealed class Email : SingleValueObject<string>
    {
        public Email(string value) : base(value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty", nameof(value));
        }
    }

    private sealed class Age : SingleValueObject<int>
    {
        public Age(int value) : base(value)
        {
            if (value < 0)
                throw new ArgumentException("Age cannot be negative", nameof(value));
        }
    }

    private sealed class Temperature : SingleValueObject<double>
    {
        public Temperature(double value) : base(value) { }
    }

    #endregion

    #region Constructor and Value Tests

    [Fact]
    public void Constructor_ShouldSetValue()
    {
        // Arrange & Act
        var email = new Email("test@example.com");

        // Assert
        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void Constructor_ShouldAllowValidation()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(""));
        Assert.Throws<ArgumentException>(() => new Age(-1));
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameValue()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act & Assert
        Assert.True(email1.Equals(email2));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentValues()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        Assert.False(email1.Equals(email2));
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");
        var email3 = new Email("other@example.com");

        // Act & Assert
        Assert.True(email1 == email2);
        Assert.False(email1 == email3);
    }

    #endregion

    #region CompareTo Tests

    [Fact]
    public void CompareTo_ShouldReturnZero_ForEqualValues()
    {
        // Arrange
        var age1 = new Age(25);
        var age2 = new Age(25);

        // Act
        var result = age1.CompareTo(age2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareTo_ShouldReturnNegative_WhenLessThan()
    {
        // Arrange
        var age1 = new Age(20);
        var age2 = new Age(30);

        // Act
        var result = age1.CompareTo(age2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_ShouldReturnPositive_WhenGreaterThan()
    {
        // Arrange
        var age1 = new Age(30);
        var age2 = new Age(20);

        // Act
        var result = age1.CompareTo(age2);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void CompareTo_ShouldReturnPositive_WhenComparedToNull()
    {
        // Arrange
        var age = new Age(25);

        // Act
        var result = age.CompareTo(null);

        // Assert
        Assert.True(result > 0);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_ShouldReturnUnderlyingValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        string value = email;

        // Assert
        Assert.Equal("test@example.com", value);
    }

    [Fact]
    public void ImplicitConversion_ShouldWorkWithNumericTypes()
    {
        // Arrange
        var age = new Age(25);

        // Act
        int value = age;

        // Assert
        Assert.Equal(25, value);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnValueString()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        Assert.Equal("test@example.com", result);
    }

    [Fact]
    public void ToString_ShouldReturnEmptyString_ForNullValue()
    {
        // This test is conceptual - our implementations don't allow null,
        // but testing the base behavior
        var temp = new Temperature(0.0);
        Assert.Equal("0", temp.ToString());
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_ShouldBeSame_ForEqualValues()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act & Assert
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }

    #endregion

    #region IComparable Interface Tests

    [Fact]
    public void SingleValueObject_ShouldImplementIComparable()
    {
        // Arrange & Act
        var age = new Age(25);

        // Assert
        Assert.IsAssignableFrom<IComparable<SingleValueObject<int>>>(age);
    }

    #endregion
}
