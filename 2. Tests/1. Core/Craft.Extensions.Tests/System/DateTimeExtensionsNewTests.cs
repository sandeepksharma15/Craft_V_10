namespace Craft.Extensions.Tests.System;

public class DateTimeExtensionsNewTests
{
    [Fact]
    public void CalculateAge_WithBirthdayNotYetThisYear_ReturnsCorrectAge()
    {
        // Arrange
        var birthDate = new DateTime(1990, 12, 25);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var age = birthDate.CalculateAge(referenceDate);

        // Assert
        Assert.Equal(32, age);
    }

    [Fact]
    public void CalculateAge_WithBirthdayAlreadyPassedThisYear_ReturnsCorrectAge()
    {
        // Arrange
        var birthDate = new DateTime(1990, 3, 15);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var age = birthDate.CalculateAge(referenceDate);

        // Assert
        Assert.Equal(33, age);
    }

    [Fact]
    public void CalculateAge_OnExactBirthday_ReturnsCorrectAge()
    {
        // Arrange
        var birthDate = new DateTime(1990, 6, 15);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var age = birthDate.CalculateAge(referenceDate);

        // Assert
        Assert.Equal(33, age);
    }

    [Fact]
    public void StartOfMonth_ReturnsFirstDayOfMonth()
    {
        // Arrange
        var date = new DateTime(2023, 6, 15, 14, 30, 45);

        // Act
        var result = date.StartOfMonth();

        // Assert
        Assert.Equal(new DateTime(2023, 6, 1), result);
    }

    [Fact]
    public void EndOfMonth_ReturnsLastDayOfMonth()
    {
        // Arrange
        var date = new DateTime(2023, 6, 15);

        // Act
        var result = date.EndOfMonth();

        // Assert
        Assert.Equal(new DateTime(2023, 6, 30, 23, 59, 59, 999), result);
    }

    [Fact]
    public void StartOfYear_ReturnsFirstDayOfYear()
    {
        // Arrange
        var date = new DateTime(2023, 6, 15, 14, 30, 45);

        // Act
        var result = date.StartOfYear();

        // Assert
        Assert.Equal(new DateTime(2023, 1, 1), result);
    }

    [Fact]
    public void EndOfYear_ReturnsLastDayOfYear()
    {
        // Arrange
        var date = new DateTime(2023, 6, 15);

        // Act
        var result = date.EndOfYear();

        // Assert
        Assert.Equal(new DateTime(2023, 12, 31, 23, 59, 59, 999), result);
    }

    [Theory]
    [InlineData("2023-06-15", DayOfWeek.Monday, "2023-06-12")] // Thursday -> Monday
    [InlineData("2023-06-12", DayOfWeek.Monday, "2023-06-12")] // Monday -> Monday
    [InlineData("2023-06-18", DayOfWeek.Monday, "2023-06-12")] // Sunday -> Monday
    public void StartOfWeek_ReturnsCorrectDate(string dateStr, DayOfWeek startOfWeek, string expectedStr)
    {
        // Arrange
        var date = DateTime.Parse(dateStr);
        var expected = DateTime.Parse(expectedStr);

        // Act
        var result = date.StartOfWeek(startOfWeek);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5, "2023-06-22")] // Add 5 business days from Thursday (Jun 15) -> Thu Jun 22
    [InlineData(-5, "2023-06-08")] // Subtract 5 business days from Thursday
    [InlineData(0, "2023-06-15")] // Add 0 business days
    public void AddBusinessDays_ReturnsCorrectDate(int businessDays, string expectedStr)
    {
        // Arrange
        var startDate = new DateTime(2023, 6, 15); // Thursday
        var expected = DateTime.Parse(expectedStr);

        // Act
        var result = startDate.AddBusinessDays(businessDays);

        // Assert
        Assert.Equal(expected.Date, result.Date);
    }

    [Theory]
    [InlineData("2023-06-15", true)]  // Thursday
    [InlineData("2023-06-17", false)] // Saturday
    [InlineData("2023-06-18", false)] // Sunday
    public void IsWeekday_DateTime_ReturnsCorrectResult(string dateStr, bool expected)
    {
        // Arrange
        var date = DateTime.Parse(dateStr);

        // Act
        var result = date.IsWeekday();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("2023-06-15", false)] // Thursday
    [InlineData("2023-06-17", true)]  // Saturday
    [InlineData("2023-06-18", true)]  // Sunday
    public void IsWeekend_DateTime_ReturnsCorrectResult(string dateStr, bool expected)
    {
        // Arrange
        var date = DateTime.Parse(dateStr);

        // Act
        var result = date.IsWeekend();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsToday_WithToday_ReturnsTrue()
    {
        // Arrange
        var date = DateTime.Today;

        // Act
        var result = date.IsToday();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsToday_WithYesterday_ReturnsFalse()
    {
        // Arrange
        var date = DateTime.Today.AddDays(-1);

        // Act
        var result = date.IsToday();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsPast_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var date = DateTime.Now.AddDays(-1);

        // Act
        var result = date.IsPast();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsFuture_WithFutureDate_ReturnsTrue()
    {
        // Arrange
        var date = DateTime.Now.AddDays(1);

        // Act
        var result = date.IsFuture();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SetTime_SetsTimeCorrectly()
    {
        // Arrange
        var date = new DateTime(2023, 6, 15);

        // Act
        var result = date.SetTime(14, 30, 45, 500);

        // Assert
        Assert.Equal(new DateTime(2023, 6, 15, 14, 30, 45, 500), result);
    }

    [Fact]
    public void At_SetsTimeFromTimeSpan()
    {
        // Arrange
        var date = new DateTime(2023, 6, 15, 10, 20, 30);
        var time = new TimeSpan(14, 30, 45);

        // Act
        var result = date.At(time);

        // Assert
        Assert.Equal(new DateTime(2023, 6, 15, 14, 30, 45), result);
    }

    [Theory]
    [InlineData("2023-06-15", "2023-06-10", "2023-06-20", true)]
    [InlineData("2023-06-15", "2023-06-15", "2023-06-20", true)]
    [InlineData("2023-06-15", "2023-06-10", "2023-06-15", true)]
    [InlineData("2023-06-15", "2023-06-16", "2023-06-20", false)]
    [InlineData("2023-06-15", "2023-06-10", "2023-06-14", false)]
    public void IsBetween_ReturnsCorrectResult(string dateStr, string startStr, string endStr, bool expected)
    {
        // Arrange
        var date = DateTime.Parse(dateStr);
        var start = DateTime.Parse(startStr);
        var end = DateTime.Parse(endStr);

        // Act
        var result = date.IsBetween(start, end);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EndOfWeek_ReturnsCorrectDate()
    {
        // Arrange
        var date = new DateTime(2023, 6, 15); // Thursday

        // Act
        var result = date.EndOfWeek(DayOfWeek.Monday);

        // Assert
        Assert.Equal(DayOfWeek.Sunday, result.DayOfWeek);
        Assert.Equal(23, result.Hour);
        Assert.Equal(59, result.Minute);
        Assert.Equal(59, result.Second);
    }
}
