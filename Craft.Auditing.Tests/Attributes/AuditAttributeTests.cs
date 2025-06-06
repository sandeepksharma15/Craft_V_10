using System;
using System.Linq;
using System.Reflection;
using Craft.Auditing;
using Xunit;

namespace Craft.Auditing.Tests.Attributes
{
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
        public void AuditAttribute_CannotBeAppliedToMethodOrProperty()
        {
            // Arrange
            var methodAttr = typeof(DummyNonAuditedClass)
                .GetMethod(nameof(DummyNonAuditedClass.SomeMethod))
                .GetCustomAttribute<AuditAttribute>();

            var propAttr = typeof(DummyNonAuditedClass)
                .GetProperty(nameof(DummyNonAuditedClass.SomeProperty))
                .GetCustomAttribute<AuditAttribute>();

            // Assert
            Assert.Null(methodAttr);
            Assert.Null(propAttr);
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

        private class DummyNonAuditedClass
        {
            public void SomeMethod() { }
            public int SomeProperty { get; set; }
        }
    }
}
