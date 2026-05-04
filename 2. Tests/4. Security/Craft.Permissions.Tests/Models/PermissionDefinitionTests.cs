namespace Craft.Permissions.Tests.Models;

public class PermissionDefinitionTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var definition = new PermissionDefinition(1001, "Delete Student", "Students");

        Assert.Equal(1001, definition.Code);
        Assert.Equal("Delete Student", definition.Name);
        Assert.Equal("Students", definition.Group);
    }

    [Fact]
    public void Constructor_GroupDefaultsToNull()
    {
        var definition = new PermissionDefinition(1002, "View Reports");

        Assert.Null(definition.Group);
    }

    [Fact]
    public void Record_Equality_BasedOnValues()
    {
        var a = new PermissionDefinition(1001, "Delete Student", "Students");
        var b = new PermissionDefinition(1001, "Delete Student", "Students");

        Assert.Equal(a, b);
    }
}
