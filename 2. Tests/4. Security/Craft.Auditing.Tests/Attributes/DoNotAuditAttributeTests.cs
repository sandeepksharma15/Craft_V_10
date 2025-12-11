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
    public void DoNotAuditAttribute_CanBeAppliedToProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(DummyClassWithNonAuditedProperty).GetProperty(nameof(DummyClassWithNonAuditedProperty.NonAuditedProperty));
        var attr = propertyInfo?.GetCustomAttribute<DoNotAuditAttribute>();

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
        Assert.Equal(AttributeTargets.Class | AttributeTargets.Property, usage.ValidOn);
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

    [Fact]
    public void DoNotAuditAttribute_PropertyExtension_DetectsAttributeCorrectly()
    {
        // Arrange
        var propertyInfo = typeof(DummyClassWithNonAuditedProperty).GetProperty(nameof(DummyClassWithNonAuditedProperty.NonAuditedProperty));

        // Act
        var hasAttribute = propertyInfo?.HasDoNotAuditAttribute();

        // Assert
        Assert.True(hasAttribute);
    }

    [Fact]
    public void DoNotAuditAttribute_PropertyExtension_ReturnsFalseForRegularProperty()
    {
        // Arrange
        var propertyInfo = typeof(DummyClassWithNonAuditedProperty).GetProperty(nameof(DummyClassWithNonAuditedProperty.RegularProperty));

        // Act
        var hasAttribute = propertyInfo?.HasDoNotAuditAttribute();

        // Assert
        Assert.False(hasAttribute);
    }

    [DoNotAudit]
    private class DummyNonAuditedClass { }

    [DoNotAudit]
    private class BaseClass { }

    private class DerivedClass : BaseClass { }

    private class DummyClassWithNonAuditedProperty
    {
        [DoNotAudit]
        public string NonAuditedProperty { get; set; } = string.Empty;

        public string RegularProperty { get; set; } = string.Empty;
    }
}
