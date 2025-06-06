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

    [Audit]
    private class DummyAuditedClass { }
}
