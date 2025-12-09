namespace Craft.Extensions.Tests.System;

public class DateTimeTests
{
    [Fact]
    public void ClearTime_Should_Not_Modify_Date_With_Time_Already_At_Midnight()
    {
        // Arrange
        var dateTime = new DateTime(2023, 6, 25, 0, 0, 0, 0);

        // Act
        var result = dateTime.ClearTime();

        // Assert
        Assert.Equal(new DateTime(2023, 6, 25), result);
    }

    [Fact]
    public void ClearTime_Should_Not_Modify_Date_With_Time_Already_Set()
    {
        // Arrange
        var dateTime = new DateTime(2023, 6, 25, 8, 30, 15, 500);

        // Act
        var result = dateTime.ClearTime();

        // Assert
        Assert.Equal(new DateTime(2023, 6, 25), result);
    }

    [Fact]
    public void ClearTime_Should_Set_Time_To_Midnight()
    {
        // Arrange
        var dateTime = new DateTime(2023, 6, 25, 15, 30, 45, 123);

        // Act
        var result = dateTime.ClearTime();

        // Assert
        Assert.Equal(new DateTime(2023, 6, 25), result);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, true)]
    [InlineData(DayOfWeek.Tuesday, true)]
    [InlineData(DayOfWeek.Wednesday, true)]
    [InlineData(DayOfWeek.Thursday, true)]
    [InlineData(DayOfWeek.Friday, true)]
    [InlineData(DayOfWeek.Saturday, false)]
    [InlineData(DayOfWeek.Sunday, false)]
    public void IsWeekDay_Should_Return_Correct_Result(DayOfWeek dayOfWeek, bool expectedResult)
    {
        // Act
        var result = dayOfWeek.IsWeekday();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, false)]
    [InlineData(DayOfWeek.Tuesday, false)]
    [InlineData(DayOfWeek.Wednesday, false)]
    [InlineData(DayOfWeek.Thursday, false)]
    [InlineData(DayOfWeek.Friday, false)]
    [InlineData(DayOfWeek.Saturday, true)]
    [InlineData(DayOfWeek.Sunday, true)]
    public void IsWeekend_Should_Return_Correct_Result(DayOfWeek dayOfWeek, bool expectedResult)
    {
        // Act
        var result = dayOfWeek.IsWeekend();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void LocalKindIsOverwrittenTest()
    {
        // Arrange
        DateTime? input = DateTime.Now;
        DateTime withKindUtcInput = DateTime.SpecifyKind(input.Value, DateTimeKind.Utc);

        // Act
        DateTime? result = withKindUtcInput.SetKindUtc();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }

    [Fact]
    public void SetKindUtcNonNullOffsetDateInputTest()
    {
        DateTime? input = DateTime.Now;
        DateTime withKindUtcInput = DateTime.SpecifyKind(input.Value, DateTimeKind.Utc);
        DateTime? result = withKindUtcInput.SetKindUtc();
        Assert.NotNull(result);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }

    [Fact]
    public void SetKindUtcNonNullRegularDateInputTest()
    {
        // Arrange
        DateTime? input = DateTime.Now;

        // Act
        DateTime? result = input.SetKindUtc();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }

    [Fact]
    public void SetKindUtcNullInputTest()
    {
        // Arrange
        DateTime? input = null;

        // Act
        DateTime? result = input?.SetKindUtc();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UnspecifiedKindIsOverwrittenTest()
    {
        // Arrange
        DateTime? input = DateTime.Now;
        DateTime withKindUtcInput = DateTime.SpecifyKind(input.Value, DateTimeKind.Unspecified);

        // Act
        DateTime? result = withKindUtcInput.SetKindUtc();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }
}
