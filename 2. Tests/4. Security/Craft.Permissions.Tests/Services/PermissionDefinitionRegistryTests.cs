namespace Craft.Permissions.Tests.Services;

public class PermissionDefinitionRegistryTests
{
    private static PermissionDefinitionRegistry CreateRegistry() => new();

    [Fact]
    public void Register_AddsDefinition_WhenCodeIsUnique()
    {
        var registry = CreateRegistry();
        var definition = new PermissionDefinition(1001, "Delete Student", "Students");

        registry.Register(definition);

        Assert.Single(registry.GetAll());
    }

    [Fact]
    public void Register_ThrowsInvalidOperationException_WhenCodeIsDuplicate()
    {
        var registry = CreateRegistry();
        registry.Register(new PermissionDefinition(1001, "Delete Student", "Students"));

        var ex = Assert.Throws<InvalidOperationException>(
            () => registry.Register(new PermissionDefinition(1001, "Another Permission", "Other")));

        Assert.Contains("1001", ex.Message);
        Assert.Contains("Delete Student", ex.Message);
    }

    [Fact]
    public void Register_ThrowsArgumentNullException_WhenDefinitionIsNull()
    {
        var registry = CreateRegistry();

        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }

    [Fact]
    public void GetAll_ReturnsEmpty_WhenNothingRegistered()
    {
        var registry = CreateRegistry();

        Assert.Empty(registry.GetAll());
    }

    [Fact]
    public void GetAll_ReturnsSortedByGroupThenName()
    {
        var registry = CreateRegistry();
        registry.Register(new PermissionDefinition(2001, "Zulu Permission", "B-Group"));
        registry.Register(new PermissionDefinition(1001, "Alpha Permission", "B-Group"));
        registry.Register(new PermissionDefinition(3001, "Middle Permission", "A-Group"));

        var all = registry.GetAll().ToList();

        Assert.Equal("A-Group", all[0].Group);
        Assert.Equal("Alpha Permission", all[1].Name);
        Assert.Equal("Zulu Permission", all[2].Name);
    }

    [Fact]
    public void GetByCode_ReturnsDefinition_WhenCodeExists()
    {
        var registry = CreateRegistry();
        var definition = new PermissionDefinition(1001, "Delete Student", "Students");
        registry.Register(definition);

        var result = registry.GetByCode(1001);

        Assert.Equal(definition, result);
    }

    [Fact]
    public void GetByCode_ReturnsNull_WhenCodeDoesNotExist()
    {
        var registry = CreateRegistry();

        Assert.Null(registry.GetByCode(9999));
    }

    [Fact]
    public void Register_AllowsMultipleUniqueDefinitions()
    {
        var registry = CreateRegistry();
        registry.Register(new PermissionDefinition(1001, "Permission A", "Group1"));
        registry.Register(new PermissionDefinition(1002, "Permission B", "Group1"));
        registry.Register(new PermissionDefinition(2001, "Permission C", "Group2"));

        Assert.Equal(3, registry.GetAll().Count);
    }
}
