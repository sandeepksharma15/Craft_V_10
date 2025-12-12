namespace Craft.Jobs.Tests;

public class CronExpressionsTests
{
    [Fact]
    public void EveryMinute_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.EveryMinute;

        // Assert
        Assert.Equal("* * * * *", cron);
    }

    [Fact]
    public void Every5Minutes_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.Every5Minutes;

        // Assert
        Assert.Equal("*/5 * * * *", cron);
    }

    [Fact]
    public void Every15Minutes_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.Every15Minutes;

        // Assert
        Assert.Equal("*/15 * * * *", cron);
    }

    [Fact]
    public void Every30Minutes_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.Every30Minutes;

        // Assert
        Assert.Equal("*/30 * * * *", cron);
    }

    [Fact]
    public void Hourly_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.Hourly;

        // Assert
        Assert.Equal("0 * * * *", cron);
    }

    [Fact]
    public void Daily_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.Daily;

        // Assert
        Assert.Equal("0 0 * * *", cron);
    }

    [Fact]
    public void DailyAt1AM_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.DailyAt1AM;

        // Assert
        Assert.Equal("0 1 * * *", cron);
    }

    [Fact]
    public void DailyAt2AM_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.DailyAt2AM;

        // Assert
        Assert.Equal("0 2 * * *", cron);
    }

    [Fact]
    public void Weekly_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.Weekly;

        // Assert
        Assert.Equal("0 0 * * 0", cron);
    }

    [Fact]
    public void Monthly_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.Monthly;

        // Assert
        Assert.Equal("0 0 1 * *", cron);
    }

    [Fact]
    public void WeekdaysAt9AM_ReturnsCorrectExpression()
    {
        // Act
        var cron = CronExpressions.WeekdaysAt9AM;

        // Assert
        Assert.Equal("0 9 * * 1-5", cron);
    }

    [Theory]
    [InlineData(1, "*/1 * * * *")]
    [InlineData(10, "*/10 * * * *")]
    [InlineData(30, "*/30 * * * *")]
    [InlineData(59, "*/59 * * * *")]
    public void EveryNMinutes_WithValidMinutes_ReturnsCorrectExpression(int minutes, string expected)
    {
        // Act
        var cron = CronExpressions.EveryNMinutes(minutes);

        // Assert
        Assert.Equal(expected, cron);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(60)]
    [InlineData(100)]
    public void EveryNMinutes_WithInvalidMinutes_ThrowsArgumentException(int minutes)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CronExpressions.EveryNMinutes(minutes));
        Assert.Contains("Minutes must be between 1 and 59", ex.Message);
    }

    [Theory]
    [InlineData(1, "0 */1 * * *")]
    [InlineData(6, "0 */6 * * *")]
    [InlineData(12, "0 */12 * * *")]
    [InlineData(23, "0 */23 * * *")]
    public void EveryNHours_WithValidHours_ReturnsCorrectExpression(int hours, string expected)
    {
        // Act
        var cron = CronExpressions.EveryNHours(hours);

        // Assert
        Assert.Equal(expected, cron);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(24)]
    [InlineData(100)]
    public void EveryNHours_WithInvalidHours_ThrowsArgumentException(int hours)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CronExpressions.EveryNHours(hours));
        Assert.Contains("Hours must be between 1 and 23", ex.Message);
    }

    [Theory]
    [InlineData(0, 0, "0 0 * * *")]
    [InlineData(1, 0, "0 1 * * *")]
    [InlineData(12, 0, "0 12 * * *")]
    [InlineData(23, 0, "0 23 * * *")]
    [InlineData(14, 30, "30 14 * * *")]
    [InlineData(9, 15, "15 9 * * *")]
    public void DailyAtHour_WithValidTime_ReturnsCorrectExpression(int hour, int minute, string expected)
    {
        // Act
        var cron = CronExpressions.DailyAtHour(hour, minute);

        // Assert
        Assert.Equal(expected, cron);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(24, 0)]
    [InlineData(25, 0)]
    public void DailyAtHour_WithInvalidHour_ThrowsArgumentException(int hour, int minute)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CronExpressions.DailyAtHour(hour, minute));
        Assert.Contains("Hour must be between 0 and 23", ex.Message);
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(0, 60)]
    [InlineData(0, 100)]
    public void DailyAtHour_WithInvalidMinute_ThrowsArgumentException(int hour, int minute)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CronExpressions.DailyAtHour(hour, minute));
        Assert.Contains("Minute must be between 0 and 59", ex.Message);
    }

    [Fact]
    public void DailyAtHour_WithDefaultMinute_UsesZero()
    {
        // Act
        var cron = CronExpressions.DailyAtHour(5);

        // Assert
        Assert.Equal("0 5 * * *", cron);
    }
}
