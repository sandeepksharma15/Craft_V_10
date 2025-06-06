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

    [DoNotAudit]
    private class DummyNonAuditedClass { }
}
