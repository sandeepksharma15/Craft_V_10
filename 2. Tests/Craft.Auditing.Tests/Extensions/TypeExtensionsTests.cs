namespace Craft.Auditing.Tests.Extensions;

public class TypeExtensionsTests
{
    [Fact]
    public void HasDisableAuditingAttribute_Should_Return_False_When_DisableAuditingAttribute_Is_Not_Present()
    {
        // Arrange
        var type = typeof(TestClassWithoutDisableAuditingAttribute);

        // Act
        var result = type.HasDoNotAuditAttribute();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasDisableAuditingAttribute_Should_Return_True_When_DisableAuditingAttribute_Is_Present()
    {
        // Arrange
        var type = typeof(TestClassWithDisableAuditingAttribute);

        // Act
        var result = type.HasDoNotAuditAttribute();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasEnableAuditingAttribute_Should_Return_False_When_EnableAuditingAttribute_Is_Not_Present()
    {
        // Arrange
        var type = typeof(TestClassWithoutEnableAuditingAttribute);

        // Act
        var result = type.HasAuditAttribute();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasEnableAuditingAttribute_Should_Return_True_When_EnableAuditingAttribute_Is_Present()
    {
        // Arrange
        var type = typeof(TestClassWithEnableAuditingAttribute);

        // Act
        var result = type.HasAuditAttribute();

        // Assert
        Assert.True(result);
    }

    [DoNotAudit]
    private class TestClassWithDisableAuditingAttribute;

    private class TestClassWithoutDisableAuditingAttribute;

    [Audit]
    private class TestClassWithEnableAuditingAttribute;

    private class TestClassWithoutEnableAuditingAttribute;
}
