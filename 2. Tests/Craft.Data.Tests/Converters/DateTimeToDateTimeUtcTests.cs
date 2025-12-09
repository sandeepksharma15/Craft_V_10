namespace Craft.Data.Tests.Converters;

public class DateTimeToDateTimeUtcTests
{
    [Fact]
    public void ConvertToProvider_Should_Set_Kind_To_Utc_When_Local()
    {
        // Arrange
        var converter = new DateTimeToDateTimeUtc();
        var localDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local);

        // Act
        var result = Assert.IsType<DateTime>(converter.ConvertToProvider(localDateTime));

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(localDateTime, result);
    }

    [Fact]
    public void ConvertToProvider_Should_Set_Kind_To_Utc_When_Unspecified()
    {
        // Arrange
        var converter = new DateTimeToDateTimeUtc();
        var unspecifiedDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);

        // Act
        var result = Assert.IsType<DateTime>(converter.ConvertToProvider(unspecifiedDateTime));

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(unspecifiedDateTime, result);
    }

    [Fact]
    public void ConvertToProvider_Should_Keep_Kind_Utc_When_Already_Utc()
    {
        // Arrange
        var converter = new DateTimeToDateTimeUtc();
        var utcDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var result = Assert.IsType<DateTime>(converter.ConvertToProvider(utcDateTime));

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(utcDateTime, result);
    }

    [Fact]
    public void ConvertFromProvider_Should_Return_DateTime_AsIs()
    {
        // Arrange
        var converter = new DateTimeToDateTimeUtc();
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local);

        // Act
        var result = Assert.IsType<DateTime>(converter.ConvertFromProvider(dateTime));

        // Assert
        Assert.Equal(dateTime, result);
        Assert.Equal(dateTime.Kind, result.Kind);
    }

    [Fact]
    public void ConvertToProvider_Should_Handle_MinValue()
    {
        // Arrange
        var converter = new DateTimeToDateTimeUtc();
        var minValue = DateTime.MinValue;

        // Act
        var result = Assert.IsType<DateTime>(converter.ConvertToProvider(minValue));

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(minValue, result);
    }

    [Fact]
    public void ConvertToProvider_Should_Handle_MaxValue()
    {
        // Arrange
        var converter = new DateTimeToDateTimeUtc();
        var maxValue = DateTime.MaxValue;

        // Act
        var result = Assert.IsType<DateTime>(converter.ConvertToProvider(maxValue));

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(maxValue, result);
    }

    [Fact]
    public void ConvertToProvider_Should_Handle_DateTime_With_Milliseconds()
    {
        // Arrange
        var converter = new DateTimeToDateTimeUtc();
        var dateTime = new DateTime(2024, 1, 1, 12, 0, 0, 123, DateTimeKind.Local);

        // Act
        var result = Assert.IsType<DateTime>(converter.ConvertToProvider(dateTime));

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(dateTime, result);
    }
}
