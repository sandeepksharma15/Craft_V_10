namespace Craft.Permissions.Tests.Models;

public class RolePermissionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var rp = new RolePermission();

        Assert.Equal(default, rp.RoleId);
        Assert.Equal(default, rp.PermissionCode);
        Assert.False(rp.IsDeleted);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var rp = new RolePermission
        {
            RoleId = 42L,
            PermissionCode = 1001
        };

        Assert.Equal(42L, rp.RoleId);
        Assert.Equal(1001, rp.PermissionCode);
    }
}
