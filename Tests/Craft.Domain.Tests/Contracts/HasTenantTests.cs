namespace Craft.Domain.Tests.Contracts;

public class HasTenantTests
{
    [Fact]
    public void IHasTenant_ColumnName_IsConstant()
    {
        // Assert
        Assert.Equal(IHasTenant.ColumnName, "TenantId");
    }

    [Fact]
    public void IHasTenant_CastsToIHasTenantOfConcreteType()
    {
        // Arrange
        ConcreteHasTenant instance = new();

        // Act & Assert
        IHasTenant castInstance = instance;
        Assert.IsType<ConcreteHasTenant>(castInstance);
    }

    [Fact]
    public void IHasTenantOfTKey_GetTenantId_ReturnsTenantIdValue()
    {
        // Arrange
        ConcreteHasTenant instance = new() { TenantId = 123 };
        IHasTenant castInstance = instance;

        // Act & Assert
        var actualId = castInstance.GetTenantId();
        Assert.Equal(123, actualId);
    }

    [Fact]
    public void IHasTenantOfTKey_SetTenantId_SetsTenantIdValue()
    {
        // Arrange
        ConcreteHasTenant instance = new() { TenantId = 123 };
        IHasTenant castInstance = instance;

        // Act
        castInstance.SetTenantId(456);

        // Assert
        Assert.Equal(456, instance.TenantId);
    }

    [Fact]
    public void IHasTenantOfTKey_IsTenantIdSet_ReturnsTrueForSetTenantId()
    {
        // Arrange
        IHasTenant castInstance = (ConcreteHasTenant)new() { TenantId = 123 };

        // Act & Assert
        bool isSet = castInstance.IsTenantIdSet();
        Assert.True(isSet);
    }

    [Fact]
    public void IHasTenantOfTKey_IsTenantIdSet_ReturnsFalseForDefaultTenantId()
    {
        // Arrange
        IHasTenant castInstance = (ConcreteHasTenant)new();

        // Act & Assert
        bool isSet = castInstance.IsTenantIdSet();
        Assert.False(isSet);
    }

    private class ConcreteHasTenant : IHasTenant
    {
        public KeyType Id { get; set; }
        public KeyType TenantId { get; set; }
    }
}
