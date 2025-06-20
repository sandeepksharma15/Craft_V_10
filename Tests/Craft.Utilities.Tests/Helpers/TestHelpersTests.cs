using Craft.Utilities.Helpers;

namespace Craft.Utilities.Tests.Helpers;

public class TestHelpersTests
{
    [Fact]
    public void AreTheySame_SimpleObjects_SameValues_NoExceptions()
    {
        // Arrange
        var obj1 = new { Name = "Test", Age = 30 };
        var obj2 = new { Name = "Test", Age = 30 };

        // Act & Assert
        TestHelpers.AreTheySame(obj1, obj2);
    }

    [Fact]
    public void AreTheySame_SimpleObjects_DifferentValues_ThrowsAssertionError()
    {
        // Arrange
        var obj1 = new { Name = "Test", Age = 30 };
        var obj2 = new { Name = "Test", Age = 25 }; // Age is different

        // Act & Assert
        Assert.Throws<Xunit.Sdk.EqualException>(() => TestHelpers.AreTheySame(obj1, obj2));
    }

    [Fact]
    public void AreTheySame_ObjectsWithCollections_SameValues_NoExceptions()
    {
        // Arrange
        var list1 = new List<string> { "Item1", "Item2" };
        var obj1 = new { Name = "Test", List = list1 };
        var obj2 = new { Name = "Test", List = new List<string> { "Item1", "Item2" } }; // Same content list

        // Act & Assert
        TestHelpers.AreTheySame(obj1, obj2);
    }

    [Fact]
    public void AreTheySame_ObjectsWithCollections_DifferentValues_ThrowsAssertionError()
    {
        // Arrange
        var list1 = new List<string> { "Item1", "Item2" };
        var obj1 = new { Name = "Test", List = list1 };
        var obj2 = new { Name = "Test", List = new List<string> { "Item1", "Item3" } }; // Different list content

        // Act & Assert
        Assert.Throws<Xunit.Sdk.EquivalentException>(() => TestHelpers.AreTheySame(obj1, obj2));
    }

    [Fact]
    public void ShouldBeSameAs_SimpleObjects_SameValues_NoExceptions()
    {
        // Arrange
        var obj1 = new { Name = "Test", Age = 30 };
        var obj2 = new { Name = "Test", Age = 30 };

        // Act & Assert
        obj1.ShouldBeSameAs(obj2);
    }

    [Fact]
    public void ShouldBeSameAs_SimpleObjects_DifferentValues_ThrowsAssertionError()
    {
        // Arrange
        var obj1 = new { Name = "Test", Age = 30 };
        var obj2 = new { Name = "Test", Age = 25 }; // Age is different

        // Act & Assert
        Assert.Throws<Xunit.Sdk.EqualException>(() => obj1.ShouldBeSameAs(obj2));
    }

    [Fact]
    public void AreTheySame_Should_Throw_Exception_When_One_Object_Is_Null()
    {
        // Arrange
        TestClass obj1 = new() { Id = 1, Name = "Object 1", Tags = null };
        TestClass obj2 = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => TestHelpers.AreTheySame(obj1, obj2));
    }

    [Fact]
    public void AreTheySame_ObjectsWithPublicFields_SameValues_NoExceptions()
    {
        // Arrange
        var obj1 = new PublicFieldClass { Id = 1, Name = "Test" };
        var obj2 = new PublicFieldClass { Id = 1, Name = "Test" };

        // Act & Assert
        TestHelpers.AreTheySame(obj1, obj2);
    }

    [Fact]
    public void AreTheySame_NestedObjects_DifferentValues_ThrowsAssertionError()
    {
        // Arrange
        var obj1 = new NestedClass { Name = "Outer", Inner = new PublicFieldClass { Id = 1, Name = "Inner" } };
        var obj2 = new NestedClass { Name = "Outer", Inner = new PublicFieldClass { Id = 2, Name = "Inner" } };

        // Act & Assert
        Assert.Throws<Xunit.Sdk.EqualException>(() => TestHelpers.AreTheySame(obj1, obj2));
    }


    [Fact]
    public void AreTheySame_ObjectsWithNullFieldsAndCollections_NoExceptions()
    {
        // Arrange
        var obj1 = new TestClass { Id = 1, Name = null, Tags = null };
        var obj2 = new TestClass { Id = 1, Name = null, Tags = null };

        // Act & Assert
        TestHelpers.AreTheySame(obj1, obj2);
    }

    [Fact]
    public void AreTheySame_EmptyObjects_NoExceptions()
    {
        // Act & Assert
        TestHelpers.AreTheySame(new EmptyClass(), new EmptyClass());
    }

    private class TestClass
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<string>? Tags { get; set; }
    }

    private class PublicFieldClass
    {
        public int Id;
        public string? Name;
    }

    private class NestedClass
    {
        public string? Name { get; set; }
        public PublicFieldClass? Inner { get; set; }
    }

    private class CyclicClass
    {
        public CyclicClass? Reference { get; set; }
    }

    private class EmptyClass { }
}
