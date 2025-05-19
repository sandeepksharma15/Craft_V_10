using Moq;

namespace Craft.Domain.Tests.Base;

public class BaseVmTests
{
    [Fact]
    public void BaseVm_ConcurrencyStamp_ShouldSetAndGet()
    {
        // Arrange
        const string testConcurrencyStamp = "test-concurrency-stamp";
        var Vm = new Mock<BaseVm>();

        //Act
        Vm.SetupProperty(x => x.ConcurrencyStamp, testConcurrencyStamp);

        //Assert
        Assert.Equal(testConcurrencyStamp, Vm.Object.ConcurrencyStamp);
    }

    [Fact]
    public void BaseVm_Id_ShouldSetAndGet()
    {
        // Arrange
        const int testId = 1;
        var Vm = new Mock<BaseVm>();

        //Act
        Vm.SetupProperty(x => x.Id, testId);

        //Assert
        Assert.Equal(testId, Vm.Object.Id);
    }

    [Fact]
    public void BaseVm_IsDeleted_ShouldSetAndGet()
    {
        // Arrange
        const bool isDeleted = true;
        var Vm = new Mock<BaseVm>();

        //Act
        Vm.SetupProperty(x => x.IsDeleted, isDeleted);

        //Assert
        Assert.Equal(isDeleted, Vm.Object.IsDeleted);
    }

    [Fact]
    public void ConcurrencyStamp_DefaultValue_ShouldBeNull()
    {
        // Arrange
        var Vm = new TestVm();

        // Act
        var concurrencyStamp = Vm.ConcurrencyStamp;

        // Assert
        Assert.Null(concurrencyStamp);
    }

    [Fact]
    public void ConcurrencyStamp_SetValue_ShouldReturnSetValue()
    {
        // Arrange
        var Vm = new TestVm();
        const string expectedConcurrencyStamp = "ABC123";

        // Act
        Vm.ConcurrencyStamp = expectedConcurrencyStamp;
        var concurrencyStamp = Vm.ConcurrencyStamp;

        // Assert
        Assert.Equal(expectedConcurrencyStamp, concurrencyStamp);
    }

    [Fact]
    public void Id_DefaultValue_ShouldBeZero()
    {
        // Arrange
        var Vm = new TestVm();

        // Act
        var id = Vm.Id;

        // Assert
        Assert.Equal(0, id);
    }

    [Fact]
    public void Id_SetValue_ShouldReturnSetValue()
    {
        // Arrange
        var Vm = new TestVm();
        const int expectedId = 1;

        // Act
        Vm.Id = expectedId;
        var id = Vm.Id;

        // Assert
        Assert.Equal(expectedId, id);
    }

    [Fact]
    public void IsDeleted_DefaultValue_ShouldBeFalse()
    {
        // Arrange
        var Vm = new TestVm();

        // Act
        var isDeleted = Vm.IsDeleted;

        // Assert
        Assert.False(isDeleted);
    }

    [Fact]
    public void IsDeleted_SetValue_ShouldReturnSetValue()
    {
        // Arrange
        var Vm = new TestVm();
        const bool expectedIsDeleted = true;

        // Act
        Vm.IsDeleted = expectedIsDeleted;
        var isDeleted = Vm.IsDeleted;

        // Assert
        Assert.Equal(expectedIsDeleted, isDeleted);
    }

    public class TestVm : BaseVm;
}
