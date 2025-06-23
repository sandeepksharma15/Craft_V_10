namespace Craft.Security.Tests.Models;

public class CraftRoleTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var role = new CraftRole<Guid>();
        Assert.True(role.IsActive);
        Assert.False(role.IsDeleted);
        Assert.Null(role.Name);
        Assert.Null(role.Description);
        Assert.Null(role.NormalizedName);
    }

    [Fact]
    public void ParameterizedConstructor_SetsProperties()
    {
        var name = "Admin";
        var desc = "Administrator role";
        var role = new CraftRole<Guid>(name, desc);
        Assert.Equal(name, role.Name);
        Assert.Equal(desc, role.Description);
        Assert.Equal(name.ToUpperInvariant(), role.NormalizedName);
        Assert.True(role.IsActive);
        Assert.False(string.IsNullOrWhiteSpace(role.ConcurrencyStamp));
    }

    [Fact]
    public void ParameterizedConstructor_SetsConcurrencyStampToGuid()
    {
        var role = new CraftRole<Guid>("User");
        Assert.True(Guid.TryParse(role.ConcurrencyStamp, out _));
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var role = new CraftRole<Guid>("Test") { IsActive = false };
        role.Activate();
        Assert.True(role.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var role = new CraftRole<Guid>("Test") { IsActive = true };
        role.Deactivate();
        Assert.False(role.IsActive);
    }

    [Fact]
    public void CanSetAndGet_Description()
    {
        var role = new CraftRole<Guid>
        {
            Description = "desc"
        };
        Assert.Equal("desc", role.Description);
    }

    [Fact]
    public void CanSetAndGet_IsDeleted()
    {
        var role = new CraftRole<Guid>
        {
            IsDeleted = true
        };
        Assert.True(role.IsDeleted);
        role.IsDeleted = false;
        Assert.False(role.IsDeleted);
    }

    [Fact]
    public void CanSetAndGet_Name_And_NormalizedName()
    {
        var role = new CraftRole<Guid>
        {
            Name = "role",
            NormalizedName = "ROLE"
        };
        Assert.Equal("role", role.Name);
        Assert.Equal("ROLE", role.NormalizedName);
    }

    [Fact]
    public void NonGenericCraftRole_Defaults()
    {
        var role = new CraftRole();
        Assert.True(role.IsActive);
        Assert.False(role.IsDeleted);
        Assert.Null(role.Name);
        Assert.Null(role.Description);
    }

    [Fact]
    public void NonGenericCraftRole_ParameterizedConstructor()
    {
        var role = new CraftRole("Manager", "Manages stuff");
        Assert.Equal("Manager", role.Name);
        Assert.Equal("Manages stuff", role.Description);
        Assert.Equal("MANAGER", role.NormalizedName);
        Assert.True(role.IsActive);
    }
}
