using System.Reflection;

namespace Craft.Auditing.Tests.Attributes;

public class DoNotAuditAttributeTests
{
    [Fact]
    public void DoNotAuditAttribute_CanBeAppliedToClass()
    {
        // Arrange & Act
        var attr = typeof(DummyNonAuditedClass).GetCustomAttribute<DoNotAuditAttribute>();

        // Assert
        Assert.NotNull(attr);
    }

    [Fact]
    public void DoNotAuditAttribute_AttributeUsage_IsCorrect()
    {
        // Arrange & Act
        var usage = typeof(DoNotAuditAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        Assert.NotNull(usage);
        Assert.Equal(AttributeTargets.Class, usage.ValidOn);
        Assert.False(usage.Inherited);
    }

    [Fact]
    public void DoNotAuditAttribute_ShouldBeAppliedToBaseClass()
    {
        // Arrange
        var baseClassType = typeof(BaseClass);

        // Act
        var hasAttribute = baseClassType.HasDoNotAuditAttribute();

        // Assert
        Assert.True(hasAttribute);
    }

    [Fact]
    public void DoNotAuditAttribute_ShouldNotBeAppliedToDerivedClass()
    {
        // Arrange
        var derivedClassType = typeof(DerivedClass);

        // Act
        var hasAttribute = derivedClassType.HasDoNotAuditAttribute();

        // Assert
        Assert.False(hasAttribute);
    }

    [DoNotAudit]
    private class DummyNonAuditedClass { }

    [DoNotAudit]
    private class BaseClass { }

    private class DerivedClass : BaseClass { }
}
