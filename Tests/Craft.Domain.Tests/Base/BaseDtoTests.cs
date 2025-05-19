using Moq;

namespace Craft.Domain.Tests.Base;

public class BaseDtoTests
{
    [Fact]
    public void BaseDto_ConcurrencyStamp_ShouldSetAndGet()
    {
        // Arrange
        const string testConcurrencyStamp = "test-concurrency-stamp";
        var dto = new Mock<BaseDto>();

        //Act
        dto.SetupProperty(x => x.ConcurrencyStamp, testConcurrencyStamp);

        //Assert
        Assert.Equal(testConcurrencyStamp, dto.Object.ConcurrencyStamp);
    }

    [Fact]
    public void BaseDto_Id_ShouldSetAndGet()
    {
        // Arrange
        const int testId = 1;
        var dto = new Mock<BaseDto>();

        //Act
        dto.SetupProperty(x => x.Id, testId);

        //Assert
        Assert.Equal(testId, dto.Object.Id);
    }

    [Fact]
    public void BaseDto_IsDeleted_ShouldSetAndGet()
    {
        // Arrange
        const bool isDeleted = true;
        var dto = new Mock<BaseDto>();

        //Act
        dto.SetupProperty(x => x.IsDeleted, isDeleted);

        //Assert
        Assert.Equal(isDeleted, dto.Object.IsDeleted);
    }

    [Fact]
    public void ConcurrencyStamp_DefaultValue_ShouldBeNull()
    {
        // Arrange
        var dto = new TestDto();

        // Act
        var concurrencyStamp = dto.ConcurrencyStamp;

        // Assert
        Assert.Null(concurrencyStamp);
    }

    [Fact]
    public void ConcurrencyStamp_SetValue_ShouldReturnSetValue()
    {
        // Arrange
        var dto = new TestDto();
        const string expectedConcurrencyStamp = "ABC123";

        // Act
        dto.ConcurrencyStamp = expectedConcurrencyStamp;
        var concurrencyStamp = dto.ConcurrencyStamp;

        // Assert
        Assert.Equal(expectedConcurrencyStamp, concurrencyStamp);
    }

    [Fact]
    public void Id_DefaultValue_ShouldBeZero()
    {
        // Arrange
        var dto = new TestDto();

        // Act
        var id = dto.Id;

        // Assert
        Assert.Equal(0, id);
    }

    [Fact]
    public void Id_SetValue_ShouldReturnSetValue()
    {
        // Arrange
        var dto = new TestDto();
        const int expectedId = 1;

        // Act
        dto.Id = expectedId;
        var id = dto.Id;

        // Assert
        Assert.Equal(expectedId, id);
    }

    [Fact]
    public void IsDeleted_DefaultValue_ShouldBeFalse()
    {
        // Arrange
        var dto = new TestDto();

        // Act
        var isDeleted = dto.IsDeleted;

        // Assert
        Assert.False(isDeleted);
    }

    [Fact]
    public void IsDeleted_SetValue_ShouldReturnSetValue()
    {
        // Arrange
        var dto = new TestDto();
        const bool expectedIsDeleted = true;

        // Act
        dto.IsDeleted = expectedIsDeleted;
        var isDeleted = dto.IsDeleted;

        // Assert
        Assert.Equal(expectedIsDeleted, isDeleted);
    }

    // TestDto class for testing the abstract BaseDto
    public class TestDto : BaseDto;
}
