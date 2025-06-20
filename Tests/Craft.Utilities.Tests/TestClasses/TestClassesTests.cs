using Craft.Utilities.TestClasses;

namespace Craft.Utilities.Tests.TestClasses;

// Minimal IType interface for testing
public interface ITestType
{
    int Id { get; set; }
    string? Name { get; set; }
}

// Test DTO
public class TestDto : ITestType
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

// Test Entity
public class TestEntity : ITestType
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

// Test VM
public class TestVm : ITestType
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

// Concrete test class for BaseMapperTests
public class ConcreteMapperTests : BaseMapperTests<TestEntity, TestDto, TestVm, ITestType>
{
    protected override TClass CreateInstance<TClass>()
    {
        if (typeof(TClass) == typeof(TestEntity))
            return (TClass)(object)new TestEntity { Id = 1, Name = "Entity" };

        if (typeof(TClass) == typeof(TestDto))
            return (TClass)(object)new TestDto { Id = 1, Name = "DTO" };

        if (typeof(TClass) == typeof(TestVm))
            return (TClass)(object)new TestVm { Id = 1, Name = "VM" };

        throw new InvalidOperationException($"Unsupported type: {typeof(TClass).Name}");
    }
}

public class TestClassesTests
{
    [Fact]
    public void DTO_To_Entity_IsValid_Works()
    {
        var test = new ConcreteMapperTests();
        test.DTO_To_Entity_IsValid();
    }

    [Fact]
    public void Entity_To_VM_IsValid_Works()
    {
        var test = new ConcreteMapperTests();
        test.Entity_To_VM_IsValid();
    }

    [Fact]
    public void VM_To_DTO_IsValid_Works()
    {
        var test = new ConcreteMapperTests();
        test.VM_To_DTO_IsValid();
    }

    [Fact]
    public void DTO_To_Entity_IsValid_NullInstance_DoesNotThrow()
    {
        var test = new NullInstanceMapperTests();
        test.DTO_To_Entity_IsValid();
    }

    [Fact]
    public void Entity_To_VM_IsValid_NullInstance_DoesNotThrow()
    {
        var test = new NullInstanceMapperTests();
        test.Entity_To_VM_IsValid();
    }

    [Fact]
    public void VM_To_DTO_IsValid_NullInstance_DoesNotThrow()
    {
        var test = new NullInstanceMapperTests();
        test.VM_To_DTO_IsValid();
    }
}

// Mapper that always returns null for CreateInstance
public class NullInstanceMapperTests : BaseMapperTests<TestEntity, TestDto, TestVm, ITestType>
{
    protected override TClass CreateInstance<TClass>() => null!;
}
