namespace Craft.Domain.Tests.Contracts;

public class HasDateRangeTests
{
    private sealed class DateRangeEntity : IHasDateRange
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    // --- IsCurrentAssignment ---

    [Fact]
    public void IsCurrentAssignment_WhenStartDateSetAndNoEndDate_ReturnsTrue()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1) };

        // Assert
        Assert.True(entity.IsCurrentAssignment);
    }

    [Fact]
    public void IsCurrentAssignment_WhenBothDatesSet_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 12, 31) };

        // Assert
        Assert.False(entity.IsCurrentAssignment);
    }

    [Fact]
    public void IsCurrentAssignment_WhenNoStartDate_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity();

        // Assert
        Assert.False(entity.IsCurrentAssignment);
    }

    // --- IsDateInRange ---

    [Fact]
    public void IsDateInRange_WhenDateWithinClosedRange_ReturnsTrue()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 12, 31) };

        // Assert
        Assert.True(entity.IsDateInRange(new DateOnly(2024, 6, 15)));
    }

    [Fact]
    public void IsDateInRange_WhenDateWithinOpenRange_ReturnsTrue()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1) };

        // Assert
        Assert.True(entity.IsDateInRange(new DateOnly(2099, 1, 1)));
    }

    [Fact]
    public void IsDateInRange_WhenDateBeforeStart_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 6, 1), EndDate = new DateOnly(2024, 12, 31) };

        // Assert
        Assert.False(entity.IsDateInRange(new DateOnly(2024, 1, 1)));
    }

    [Fact]
    public void IsDateInRange_WhenDateAfterEnd_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 6, 30) };

        // Assert
        Assert.False(entity.IsDateInRange(new DateOnly(2024, 12, 31)));
    }

    [Fact]
    public void IsDateInRange_WhenNoStartDate_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity();

        // Assert
        Assert.False(entity.IsDateInRange(new DateOnly(2024, 6, 15)));
    }

    // --- OverlapsWith(DateOnly, DateOnly) ---

    [Fact]
    public void OverlapsWith_Dates_WhenRangesOverlap_ReturnsTrue()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 6, 30) };

        // Assert
        Assert.True(entity.OverlapsWith(new DateOnly(2024, 6, 1), new DateOnly(2024, 12, 31)));
    }

    [Fact]
    public void OverlapsWith_Dates_WhenRangesDoNotOverlap_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 3, 31) };

        // Assert
        Assert.False(entity.OverlapsWith(new DateOnly(2024, 6, 1), new DateOnly(2024, 12, 31)));
    }

    [Fact]
    public void OverlapsWith_Dates_WhenEntityHasNoEndDate_ReturnsFalse()
    {
        // Arrange — open-ended ranges are excluded from overlap checks (both dates required)
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1) };

        // Assert
        Assert.False(entity.OverlapsWith(new DateOnly(2024, 6, 1), new DateOnly(2024, 12, 31)));
    }

    // --- OverlapsWith(IHasDateRange) ---

    [Fact]
    public void OverlapsWith_Other_WhenRangesOverlap_ReturnsTrue()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 6, 30) };
        IHasDateRange other = new DateRangeEntity { StartDate = new DateOnly(2024, 6, 1), EndDate = new DateOnly(2024, 12, 31) };

        // Assert
        Assert.True(entity.OverlapsWith(other));
    }

    [Fact]
    public void OverlapsWith_Other_WhenRangesDoNotOverlap_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 3, 31) };
        IHasDateRange other = new DateRangeEntity { StartDate = new DateOnly(2024, 6, 1), EndDate = new DateOnly(2024, 12, 31) };

        // Assert
        Assert.False(entity.OverlapsWith(other));
    }

    [Fact]
    public void OverlapsWith_Other_WhenOtherHasNoEndDate_ReturnsFalse()
    {
        // Arrange — both ranges need closed dates
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 6, 30) };
        IHasDateRange other = new DateRangeEntity { StartDate = new DateOnly(2024, 3, 1) };

        // Assert
        Assert.False(entity.OverlapsWith(other));
    }

    // --- IsInPast ---

    [Fact]
    public void IsInPast_WhenEndDateBeforeToday_ReturnsTrue()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2020, 1, 1), EndDate = new DateOnly(2020, 12, 31) };

        // Assert
        Assert.True(entity.IsInPast());
    }

    [Fact]
    public void IsInPast_WhenNoEndDate_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2020, 1, 1) };

        // Assert
        Assert.False(entity.IsInPast());
    }

    // --- IsInFuture ---

    [Fact]
    public void IsInFuture_WhenStartDateAfterToday_ReturnsTrue()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2099, 1, 1) };

        // Assert
        Assert.True(entity.IsInFuture());
    }

    [Fact]
    public void IsInFuture_WhenStartDateBeforeToday_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity { StartDate = new DateOnly(2020, 1, 1) };

        // Assert
        Assert.False(entity.IsInFuture());
    }

    [Fact]
    public void IsInFuture_WhenNoStartDate_ReturnsFalse()
    {
        // Arrange
        IHasDateRange entity = new DateRangeEntity();

        // Assert
        Assert.False(entity.IsInFuture());
    }
}
