namespace Craft.MultiTenant.Tests.ModelTests;

public class TenantTests
{
    [Fact]
    public void Activate_ActivatesTenant()
    {
        var tenant = new Tenant { Type = TenantType.Tenant, IsActive = false };
        
        tenant.Activate();
        
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Activate_ThrowsForHostTenant()
    {
        var tenant = new Tenant { Type = TenantType.Host, IsActive = false };
        
        var exception = Assert.Throws<InvalidOperationException>(() => tenant.Activate());
        Assert.Equal("Host tenant cannot be activated", exception.Message);
    }

    [Fact]
    public void Deactivate_DeactivatesTenant()
    {
        var tenant = new Tenant { Type = TenantType.Tenant, IsActive = true };
        
        tenant.Deactivate();
        
        Assert.False(tenant.IsActive);
    }

    [Fact]
    public void Deactivate_ThrowsForHostTenant()
    {
        var tenant = new Tenant { Type = TenantType.Host, IsActive = true };
        
        var exception = Assert.Throws<InvalidOperationException>(() => tenant.Deactivate());
        Assert.Equal("Host tenant cannot be deactivated", exception.Message);
    }

    [Fact]
    public void AddValidity_AddsMonths()
    {
        var tenant = new Tenant { ValidUpTo = new DateTime(2025, 1, 1) };
        
        tenant.AddValidity(3);
        
        Assert.Equal(new DateTime(2025, 4, 1), tenant.ValidUpTo);
    }

    [Fact]
    public void AddValidity_HandlesNegativeMonths()
    {
        var tenant = new Tenant { ValidUpTo = new DateTime(2025, 4, 1) };
        
        tenant.AddValidity(-2);
        
        Assert.Equal(new DateTime(2025, 2, 1), tenant.ValidUpTo);
    }

    [Fact]
    public void SetValidity_UpdatesValidityWhenFutureDate()
    {
        var tenant = new Tenant { ValidUpTo = new DateTime(2025, 1, 1) };
        var newDate = new DateTime(2025, 12, 31);
        
        tenant.SetValidity(newDate);
        
        Assert.Equal(newDate, tenant.ValidUpTo);
    }

    [Fact]
    public void SetValidity_ThrowsWhenBackdating()
    {
        var tenant = new Tenant { ValidUpTo = new DateTime(2025, 12, 31) };
        var pastDate = new DateTime(2025, 1, 1);
        
        var exception = Assert.Throws<Exception>(() => tenant.SetValidity(pastDate));
        Assert.Equal("Subscription cannot be backdated", exception.Message);
    }

    [Fact]
    public void Constructor_DefaultParameterless_InitializesCorrectly()
    {
        var tenant = new Tenant();
        
        Assert.Equal(default(long), tenant.Id);
        Assert.Equal(string.Empty, tenant.Name);
        Assert.Equal(string.Empty, tenant.ConnectionString);
        Assert.Equal(TenantDbType.Shared, tenant.DbType);
    }

    [Fact]
    public void Constructor_WithIdAndName_InitializesCorrectly()
    {
        var tenant = new Tenant(1, "Test", "connection", "test-id");
        
        Assert.Equal(1, tenant.Id);
        Assert.Equal("Test", tenant.Name);
        Assert.Equal("connection", tenant.ConnectionString);
        Assert.Equal("test-id", tenant.Identifier);
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Constructor_WithAllParameters_InitializesCorrectly()
    {
        var tenant = new Tenant("Test", "connection", "test-id", "logo.png", "admin@test.com");
        
        Assert.Equal("Test", tenant.Name);
        Assert.Equal("connection", tenant.ConnectionString);
        Assert.Equal("test-id", tenant.Identifier);
        Assert.Equal("logo.png", tenant.LogoUri);
        Assert.Equal("admin@test.com", tenant.AdminEmail);
    }

    [Fact]
    public void Constructor_WithType_InitializesCorrectly()
    {
        var tenant = new Tenant(1, "Test", "test-id", "logo.png", TenantType.Host);
        
        Assert.Equal(1, tenant.Id);
        Assert.Equal("Test", tenant.Name);
        Assert.Equal("test-id", tenant.Identifier);
        Assert.Equal("logo.png", tenant.LogoUri);
        Assert.Equal(TenantType.Host, tenant.Type);
    }

    [Fact]
    public void DefaultValidUpTo_IsOneMonthFromNow()
    {
        var tenant = new Tenant();
        var expectedDate = DateTime.UtcNow.AddMonths(1);
        
        Assert.True((tenant.ValidUpTo - expectedDate).TotalHours < 1);
    }

    [Fact]
    public void TenantType_CanBeSetAndRetrieved()
    {
        var tenant = new Tenant { Type = TenantType.Host };
        
        Assert.Equal(TenantType.Host, tenant.Type);
        
        tenant.Type = TenantType.Tenant;
        Assert.Equal(TenantType.Tenant, tenant.Type);
    }

    [Fact]
    public void TenantDbType_CanBeSetAndRetrieved()
    {
        var tenant = new Tenant { DbType = TenantDbType.PerTenant };
        
        Assert.Equal(TenantDbType.PerTenant, tenant.DbType);
        
        tenant.DbType = TenantDbType.Hybrid;
        Assert.Equal(TenantDbType.Hybrid, tenant.DbType);
    }
}
