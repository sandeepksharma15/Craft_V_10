using System.Reflection;

namespace Craft.Auditing.Tests.Attributes;

public class DoNotAuditAttributeTests
{
    [Fact]
    public void DoNotAuditAttribute_CanBeAppliedToClass()
    {
        var attr = typeof(DummyNonAuditedClass).GetCustomAttribute<DoNotAuditAttribute>();
        Assert.NotNull(attr);
    }

    [Fact]
    public void DoNotAuditAttribute_AttributeUsage_IsCorrect()
    {
        var usage = typeof(DoNotAuditAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>();

        Assert.NotNull(usage);
        Assert.Equal(AttributeTargets.Class, usage.ValidOn);
        Assert.False(usage.Inherited);
    }

    [DoNotAudit]
    private class DummyNonAuditedClass { }
}
