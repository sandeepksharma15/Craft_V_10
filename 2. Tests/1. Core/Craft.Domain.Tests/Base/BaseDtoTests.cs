using Moq;

namespace Craft.Domain.Tests.Base;

/// <summary>
/// Unit tests for the BaseDto class.
/// </summary>
public class BaseDtoTests
{
    [Fact]
    public void BaseDto_Id_ShouldSetAndGet()
    {
        // Arrange
        const int testId = 1;
        var dto = new Mock<BaseDto>();

        // Act
        dto.SetupProperty(x => x.Id, testId);

        // Assert
        Assert.Equal(testId, dto.Object.Id);
    }

    [Fact]
    public void BaseDto_WithGenericKey_Id_ShouldSetAndGet()
    {
        // Arrange
        const long testId = 123456L;
        var dto = new Mock<BaseDto<long>>();

        // Act
        dto.SetupProperty(x => x.Id, testId);

        // Assert
        Assert.Equal(testId, dto.Object.Id);
    }

    [Fact]
    public void Id_DefaultValue_ShouldBeDefaultForType()
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
        const int expectedId = 42;

        // Act
        dto.Id = expectedId;
        var id = dto.Id;

        // Assert
        Assert.Equal(expectedId, id);
    }

    [Fact]
    public void GenericDto_Id_SetValue_ShouldReturnSetValue()
    {
        // Arrange
        var dto = new TestGenericDto();
        const long expectedId = 999L;

        // Act
        dto.Id = expectedId;
        var id = dto.Id;

        // Assert
        Assert.Equal(expectedId, id);
    }

    [Fact]
    public void BaseDto_ImplementsIModel()
    {
        // Arrange
        var dto = new TestDto();

        // Act & Assert
        Assert.IsType<IModel>(dto, exactMatch: false);
    }

    [Fact]
    public void BaseDto_WithGenericKey_ImplementsIModel()
    {
        // Arrange
        var dto = new TestGenericDto();

        // Act & Assert
        Assert.IsType<IModel<long>>(dto, exactMatch: false);
    }

    [Fact]
    public void BaseDto_DefaultConstructor_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new TestDto());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void BaseDto_WithString_Id_ShouldSetAndGet()
    {
        // Arrange
        var dto = new TestStringDto();
        const string expectedId = "test-id-123";

        // Act
        dto.Id = expectedId;

        // Assert
        Assert.Equal(expectedId, dto.Id);
    }

    [Fact]
    public void BaseDto_WithGuid_Id_ShouldSetAndGet()
    {
        // Arrange
        var dto = new TestGuidDto();
        var expectedId = Guid.NewGuid();

        // Act
        dto.Id = expectedId;

        // Assert
        Assert.Equal(expectedId, dto.Id);
    }

    [Fact]
    public void BaseDto_IsSerializable()
    {
        // Arrange & Act & Assert
        var type = typeof(BaseDto);
        var attributes = type.GetCustomAttributes(typeof(SerializableAttribute), false);
        Assert.NotEmpty(attributes);
    }

    [Fact]
    public void BaseDto_Generic_IsSerializable()
    {
        // Arrange & Act & Assert
        var type = typeof(BaseDto<>);
        var attributes = type.GetCustomAttributes(typeof(SerializableAttribute), false);
        Assert.NotEmpty(attributes);
    }

    // Test classes for testing the abstract BaseDto
    public class TestDto : BaseDto;

    public class TestGenericDto : BaseDto<long>;

    public class TestStringDto : BaseDto<string>
    {
        public override string Id { get; set; } = string.Empty;
    }

    public class TestGuidDto : BaseDto<Guid>;
}
