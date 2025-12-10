using System.Linq.Expressions;

namespace Craft.Expressions.Tests;

public class ExpressionSerializerTests
{
    private class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public TestClass? Child { get; set; }
    }

    private static ExpressionSerializer<TestClass> Serializer => new();

    [Fact]
    public void Serialize_SimpleEqualityExpression_ReturnsExpectedString()
    {
        // Arrange
        Expression<Func<TestClass, bool>> expr = x => x.Name == "John";

        // Act
        var result = Serializer.Serialize(expr);

        // Assert
        Assert.Equal("(Name == \"John\")", result);
    }

    [Fact]
    public void Serialize_ComplexExpression_ReturnsExpectedString()
    {
        // Arrange
        Expression<Func<TestClass, bool>> expr = x => x.Age > 18 && x.Name != "Jane";

        // Act
        var result = Serializer.Serialize(expr);

        // Assert
        Assert.Equal("((Age > 18) && (Name != \"Jane\"))", result);
    }

    [Fact]
    public void Deserialize_SimpleEqualityExpression_ReturnsEquivalentExpression()
    {
        // Arrange
        var exprString = "Name == \"John\"";
        var obj = new TestClass { Name = "John" };

        // Act
        var expr = Serializer.Deserialize(exprString);
        var func = expr.Compile();
        var result = func(obj);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Deserialize_ComplexExpression_ReturnsEquivalentExpression()
    {
        // Arrange
        var exprString = "Age > 18 && Name != \"Jane\"";
        var obj = new TestClass { Age = 25, Name = "John" };

        // Act
        var expr = Serializer.Deserialize(exprString);
        var func = expr.Compile();
        var result = func(obj);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_PreservesLogic()
    {
        // Arrange
        Expression<Func<TestClass, bool>> expr = x => x.Age >= 21 || x.IsActive;
        var obj1 = new TestClass { Age = 22, IsActive = false };
        var obj2 = new TestClass { Age = 18, IsActive = true };
        var obj3 = new TestClass { Age = 18, IsActive = false };

        // Act
        var str = Serializer.Serialize(expr);
        var deserialized = Serializer.Deserialize(str);
        var func = deserialized.Compile();

        // Assert
        Assert.True(func(obj1));
        Assert.True(func(obj2));
        Assert.False(func(obj3));
    }

    [Fact]
    public void Deserialize_MemberAccessAndMethodCall_Works()
    {
        // Arrange
        var exprString = "Name.Contains(\"oh\")";
        var obj = new TestClass { Name = "John" };

        // Act
        var expr = Serializer.Deserialize(exprString);
        var func = expr.Compile();
        var result = func(obj);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Deserialize_NestedMemberAccess_Works()
    {
        // Arrange
        var exprString = "Child.Name == \"Jane\"";
        var obj = new TestClass { Child = new TestClass { Name = "Jane" } };

        // Act
        var expr = Serializer.Deserialize(exprString);
        var func = expr.Compile();
        var result = func(obj);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Deserialize_ConstantTypes_Int_Bool_Null()
    {
        // Arrange
        var exprStringInt = "Age == 42";
        var exprStringBool = "IsActive == true";
        var exprStringNull = "Child == null";
        var obj1 = new TestClass { Age = 42 };
        var obj2 = new TestClass { IsActive = true };
        var obj3 = new TestClass { Child = null };

        // Act
        var exprInt = Serializer.Deserialize(exprStringInt);
        var exprBool = Serializer.Deserialize(exprStringBool);
        var exprNull = Serializer.Deserialize(exprStringNull);

        // Assert
        Assert.True(exprInt.Compile()(obj1));
        Assert.True(exprBool.Compile()(obj2));
        Assert.True(exprNull.Compile()(obj3));
    }

    [Fact]
    public void Deserialize_InvalidExpression_Throws()
    {
        // Arrange
        var exprString = "Name === \"John\"";

        // Act & Assert
        Assert.Throws<ExpressionTokenizationException>(() => Serializer.Deserialize(exprString));
    }

    [Fact]
    public void Deserialize_UnsupportedMethod_Throws()
    {
        // Arrange
        var exprString = "Name.NotAMethod()";

        // Act & Assert
        Assert.Throws<ExpressionEvaluationException>(() => Serializer.Deserialize(exprString));
    }
}
