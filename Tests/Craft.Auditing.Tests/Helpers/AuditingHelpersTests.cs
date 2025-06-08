using Craft.Domain;
using Xunit;
using System.Collections.Generic;

namespace Craft.Auditing.Tests.Helpers;

public class AuditingHelpersTests
{
    [Fact]
    public void GetAuditableBaseEntityTypes_Returns_Only_Audited_Entities_DerivedFromBaseEntity()
    {
        // Arrange & Act
        IList<string> result = AuditingHelpers.GetAuditableBaseEntityTypes();

        // Assert
        Assert.Contains(nameof(AuditedEntity), result);
        Assert.DoesNotContain(nameof(NonAuditedEntity), result);
        Assert.DoesNotContain(nameof(NoAttributeEntity), result);
        Assert.DoesNotContain(nameof(NotBaseEntityWithAudit), result);
    }

    [Fact]
    public void GetNonAuditableBaseEntityTypes_Returns_Only_NonAudited_Entities_DerivedFromBaseEntity()
    {
        // Arrange & Act
        IList<string> result = AuditingHelpers.GetNonAuditableBaseEntityTypes();

        // Assert
        Assert.Contains(nameof(NonAuditedEntity), result);
        Assert.DoesNotContain(nameof(AuditedEntity), result);
        Assert.DoesNotContain(nameof(NoAttributeEntity), result);
        Assert.DoesNotContain(nameof(NotBaseEntityWithDoNotAudit), result);
    }

    [Fact]
    public void GetAuditableNonBaseEntityTypes_Returns_Only_Audited_Entities_NotDerivedFromBaseEntity()
    {
        // Arrange & Act
        IList<string> result = AuditingHelpers.GetAuditableNonBaseEntityTypes();

        // Assert
        Assert.Contains(nameof(NotBaseEntityWithAudit), result);
        Assert.DoesNotContain(nameof(AuditedEntity), result);
        Assert.DoesNotContain(nameof(NonAuditedEntity), result);
        Assert.DoesNotContain(nameof(NoAttributeEntity), result);
    }

    [Fact]
    public void GetNonAuditableNonBaseEntityTypes_Returns_Only_NonAudited_Entities_NotDerivedFromBaseEntity()
    {
        // Arrange & Act
        IList<string> result = AuditingHelpers.GetNonAuditableNonBaseEntityTypes();

        // Assert
        Assert.Contains(nameof(NotBaseEntityWithDoNotAudit), result);
        Assert.DoesNotContain(nameof(NonAuditedEntity), result);
        Assert.DoesNotContain(nameof(AuditedEntity), result);
        Assert.DoesNotContain(nameof(NoAttributeEntity), result);
    }

    [Fact]
    public void GetTypesWithAttribute_Can_Include_Abstract_Classes()
    {
        // Arrange & Act
        IList<string> result = AuditingHelpers.GetTypesWithAttribute<BaseEntity, AuditAttribute>(includeAbstract: true);

        // Assert
        Assert.Contains(nameof(AbstractAuditedEntity), result);
    }

    [Fact]
    public void GetTypesWithAttribute_Can_Exclude_Abstract_Classes()
    {
        // Arrange & Act
        IList<string> result = AuditingHelpers.GetTypesWithAttribute<BaseEntity, AuditAttribute>(includeAbstract: false);

        // Assert
        Assert.DoesNotContain(nameof(AbstractAuditedEntity), result);
    }

    [Fact]
    public void GetTypesWithoutAttribute_Returns_Entities_Without_Attribute_DerivedFromBaseEntity()
    {
        // Arrange & Act
        IList<string> result = AuditingHelpers.GetTypesWithoutAttribute<BaseEntity, AuditAttribute>();

        // Assert
        Assert.Contains(nameof(NoAttributeEntity), result);
        Assert.Contains(nameof(NonAuditedEntity), result);
        Assert.DoesNotContain(nameof(AuditedEntity), result);
    }

    [Fact]
    public void GetTypesWithoutAttribute_Returns_Entities_Without_Attribute_NotDerivedFromBaseEntity()
    {
        // Arrange & Act
        IList<string> result = AuditingHelpers.GetTypesWithoutAttribute<BaseEntity, AuditAttribute>(mustInheritFromBaseType: false);

        // Assert
        Assert.Contains(nameof(NotBaseEntityWithoutAttribute), result);
        Assert.DoesNotContain(nameof(NotBaseEntityWithAudit), result);
    }

    // Dummy auditable entity
    [Audit]
    private class AuditedEntity : BaseEntity { }

    // Dummy non-auditable entity
    [DoNotAudit]
    private class NonAuditedEntity : BaseEntity { }

    // Dummy entity with no attribute
    private class NoAttributeEntity : BaseEntity { }

    // Entity with [Audit] but does NOT inherit from BaseEntity
    [Audit]
    private class NotBaseEntityWithAudit { }

    // Entity with [DoNotAudit] but does NOT inherit from BaseEntity
    [DoNotAudit]
    private class NotBaseEntityWithDoNotAudit { }

    // Entity with no attribute and not inheriting from BaseEntity
    private class NotBaseEntityWithoutAttribute { }

    // Abstract entity with [Audit] and inherits from BaseEntity
    [Audit]
    private abstract class AbstractAuditedEntity : BaseEntity { }
}
