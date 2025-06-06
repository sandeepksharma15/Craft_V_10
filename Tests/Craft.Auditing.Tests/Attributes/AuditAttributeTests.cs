using System.Reflection;

namespace Craft.Auditing.Tests.Attributes;

public class AuditAttributeTests
{
    [Fact]
    public void AuditAttribute_CanBeAppliedToClass()
    {
        // Arrange & Act
        var attr = typeof(DummyAuditedClass).GetCustomAttribute<AuditAttribute>();

        // Assert
        Assert.NotNull(attr);
    }

    [Fact]
    public void AuditAttribute_AttributeUsage_IsCorrect()
    {
        // Arrange
        var usage = typeof(AuditAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        Assert.NotNull(usage);
        Assert.Equal(AttributeTargets.Class, usage.ValidOn);
        Assert.False(usage.Inherited);
    }

    [Fact]
    public void AuditAttribute_ShouldBeAppliedToBaseClass()
    {
        // Arrange
        var baseClassType = typeof(BaseClass);

        // Act
        var hasAttribute = baseClassType.HasAuditAttribute();

        // Assert
        Assert.True(hasAttribute);
    }

    [Fact]
    public void AuditAttribute_ShouldNotBeAppliedToDerivedClass()
    {
        // Arrange
        var derivedClassType = typeof(DerivedClass);

        // Act
        var hasAttribute = derivedClassType.HasAuditAttribute();

        // Assert
        Assert.False(hasAttribute);
    }

    [Audit]
    private class DummyAuditedClass { }

    [Audit]
    private class BaseClass { }

    private class DerivedClass : BaseClass { }

}
