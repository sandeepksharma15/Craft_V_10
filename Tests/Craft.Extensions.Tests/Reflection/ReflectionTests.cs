using System.Linq.Expressions;
using System.Reflection;

namespace Craft.Extensions.Tests.Reflection;

public class ReflectionExtensionsTests
{
    [Fact]
    public void GetMemberByName_ReturnsDescriptor_ForSimpleProperty()
    {
        // Arrange & Act
        var desc = typeof(Simple).GetMemberByName("IntProp");

        // Assert
        Assert.NotNull(desc);
        Assert.Equal("IntProp", desc!.Name);
    }

    [Fact]
    public void GetMemberByName_ReturnsDescriptor_ForNestedProperty()
    {
        // Arrange & Act
        var desc = typeof(Simple).GetMemberByName("Nested.DoubleProp");

        // Assert
        Assert.NotNull(desc);
        Assert.Equal("DoubleProp", desc!.Name);
    }

    [Fact]
    public void GetMemberByName_ReturnsNull_IfNotFound()
    {
        // Arrange & Act
        var desc = typeof(Simple).GetMemberByName("NotExist");

        // Assert
        Assert.Null(desc);
    }

    [Fact]
    public void GetMemberByName_ThrowsOnNullType()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ReflectionExtensions.GetMemberByName(null!, "IntProp"));
    }

    [Fact]
    public void GetMemberByName_ThrowsOnNullOrEmptyName()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => typeof(Simple).GetMemberByName(null!));
        Assert.Throws<ArgumentException>(() => typeof(Simple).GetMemberByName(""));
        Assert.Throws<ArgumentException>(() => typeof(Simple).GetMemberByName("   "));
    }

    [Fact]
    public void GetMemberName_ReturnsPropertyName()
    {
        // Arrange
        Expression<Func<Simple, int>> expr = s => s.IntProp;

        // Act
        var name = expr.GetMemberName();

        // Assert
        Assert.Equal("IntProp", name);
    }

    [Fact]
    public void GetMemberType_ReturnsUnderlyingType()
    {
        // Arrange
        Expression<Func<Simple, int>> expr = s => s.IntProp;

        // Act
        var type = expr.GetMemberType();

        // Assert
        Assert.Equal(typeof(int), type);
    }

    [Fact]
    public void GetMemberType_ReturnsNonNullableType()
    {
        // Arrange
        Expression<Func<NullableHolder, int?>> expr = s => s.NullableInt;

        // Act
        var type = expr.GetMemberType();

        // Assert
        Assert.Equal(typeof(int), type);
    }

    [Fact]
    public void GetPropertyInfo_ByTypeAndName_ReturnsPropertyInfo()
    {
        // Arrange & Act
        var prop = typeof(Simple).GetPropertyInfo("IntProp");

        // Assert
        Assert.NotNull(prop);
        Assert.Equal("IntProp", prop.Name);
    }

    [Fact]
    public void GetPropertyInfo_ByTypeAndName_ThrowsIfNotFound()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => typeof(Simple).GetPropertyInfo("NotExist"));
    }

    [Fact]
    public void GetPropertyInfo_ByTypeAndName_ThrowsOnNulls()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ReflectionExtensions.GetPropertyInfo(null!, "IntProp"));
        Assert.Throws<ArgumentNullException>(() => typeof(Simple).GetPropertyInfo(null!));
        Assert.Throws<ArgumentException>(() => typeof(Simple).GetPropertyInfo(""));
        Assert.Throws<ArgumentException>(() => typeof(Simple).GetPropertyInfo("   "));
    }

    [Fact]
    public void GetPropertyInfo_FromExpression_ReturnsPropertyInfo()
    {
        // Arrange
        Expression<Func<Simple, int>> expr = s => s.IntProp;

        // Act
        var prop = expr.GetPropertyInfo();

        // Assert
        Assert.NotNull(prop);
        Assert.Equal("IntProp", prop.Name);
    }

    [Fact]
    public void GetPropertyInfo_FromObjectExpression_ReturnsPropertyInfo()
    {
        Expression<Func<Simple, object>> expr = s => s.StringProp!;
        var prop = expr.GetPropertyInfo();
        Assert.NotNull(prop);
        Assert.Equal("StringProp", prop.Name);
    }

    [Fact]
    public void GetPropertyInfo_FromLambdaExpression_ReturnsPropertyInfo()
    {
        LambdaExpression expr = (Expression<Func<Simple, int>>)(s => s.IntProp);
        var prop = expr.GetPropertyInfo();
        Assert.NotNull(prop);
        Assert.Equal("IntProp", prop.Name);
    }

    [Fact]
    public void GetPropertyInfo_ThrowsOnInvalidExpression()
    {
        Expression<Func<Simple, int>> expr = s => s.IntProp + 1;
        Assert.Throws<ArgumentException>(() => expr.GetPropertyInfo());
    }

    [Fact]
    public void GetAllProperties_ReturnsAllIncludingBase()
    {
        var props = typeof(Derived).GetAllProperties().ToList();
        Assert.Contains(props, p => p.Name == "BaseProp");
        Assert.Contains(props, p => p.Name == "DerivedProp");
    }

    private class Base { public int BaseProp { get; set; } }
    private class Derived : Base { public int DerivedProp { get; set; } }

    [Fact]
    public void SetPropertyValue_SetsPublicProperty()
    {
        var obj = new Simple();
        obj.SetPropertyValue("IntProp", 42);
        Assert.Equal(42, obj.IntProp);
    }

    [Fact]
    public void SetPropertyValue_SetsPrivateProperty()
    {
        var obj = new Simple();
        obj.SetPropertyValue("PrivateProp", 99);
        Assert.Equal(99, obj.GetPrivateProp());
    }

    [Fact]
    public void SetPropertyValue_ThrowsIfNotFoundOrNotWritable()
    {
        var obj = new Simple();
        Assert.Throws<ArgumentException>(() => obj.SetPropertyValue("NotExist", 1));
    }

    [Fact]
    public void GetPropertyValue_GetsPublicProperty()
    {
        var obj = new Simple { IntProp = 123 };
        var value = obj.GetPropertyValue("IntProp");
        Assert.Equal(123, value);
    }

    [Fact]
    public void GetPropertyValue_GetsPrivateProperty()
    {
        var obj = new Simple();
        obj.SetPrivateProp(55);
        var value = obj.GetPropertyValue("PrivateProp");
        Assert.Equal(55, value);
    }

    [Fact]
    public void GetPropertyValue_ThrowsIfNotFound()
    {
        var obj = new Simple();
        Assert.Throws<ArgumentException>(() => obj.GetPropertyValue("NotExist"));
    }

    [Fact]
    public void GetClone_DeepClonesObject()
    {
        var original = new CustomClone { Name = "A", Child = new CustomClone { Name = "B" } };
        var clone = original.GetClone();
        Assert.NotSame(original, clone);
        Assert.NotSame(original.Child, clone.Child);
        Assert.Equal("A", clone?.Name);
        Assert.Equal("B", clone?.Child?.Name);
    }

    [Fact]
    public void GetClone_ThrowsOnNull()
    {
        CustomClone obj = null!;
        Assert.Throws<ArgumentNullException>(() => obj.GetClone());
    }

    private class Simple
    {
        public int IntProp { get; set; }
        public string? StringProp { get; set; }
        public Nested? Nested { get; set; }
        private int PrivateProp { get; set; }
        public int GetPrivateProp() => PrivateProp;
        public void SetPrivateProp(int value) => PrivateProp = value;
    }

    private class Nested
    {
        public double DoubleProp { get; set; }
    }

    private class CustomClone
    {
        public string? Name { get; set; }
        public CustomClone? Child { get; set; }
    }

    private class NullableHolder 
    { 
        public int? NullableInt { get; set; } 
    }
}
