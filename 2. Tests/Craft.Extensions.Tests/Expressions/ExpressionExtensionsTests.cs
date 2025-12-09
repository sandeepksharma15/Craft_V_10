using System.Linq.Expressions;
using Craft.Extensions.Expressions;

namespace Craft.Extensions.Tests.Expressions;

public class ExpressionExtensionsTests
{
    [Fact]
    public void CreateMemberExpression_NonexistentProperty_ShouldThrowArgumentException()
    {
        // Arrange
        const string propertyName = "NonexistentProperty";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => propertyName.CreateMemberExpression<MyClass>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateMemberExpression_NullOrEmptyPropertyName_ShouldThrowArgumentException(string? propertyName)
    {
        // Arrange & Act & Assert
        if (propertyName is null)
            Assert.Throws<ArgumentNullException>(() => propertyName!.CreateMemberExpression<MyClass>());
        else
            Assert.Throws<ArgumentException>(() => propertyName!.CreateMemberExpression<MyClass>());
    }

    [Fact]
    public void CreateMemberExpression_ValidProperty_ReturnsExpression()
    {
        // Arrange
        var obj = new MyClass { PropertyName = "Test", AnotherProperty = 42 };
        const string propertyName = "PropertyName";

        // Act
        var expression = propertyName.CreateMemberExpression<MyClass>();

        // Assert
        Assert.NotNull(expression);

        // Arrange & Act
        var compiled = expression.Compile();
        var value = compiled.DynamicInvoke(obj);

        // Assert
        Assert.Equal("Test", value);
    }

    [Fact]
    public void CreateMemberExpression_ValidField_ReturnsExpression()
    {
        // Arrange
        var type = typeof(MyClass);
        var fieldName = nameof(MyClass.StaticField);
        MyClass.StaticField = 123;

        // Act
        var expr = type.CreateMemberExpression(fieldName);
        var compiled = expr.Compile();
        var result = compiled.DynamicInvoke(); // No arguments for static

        // Assert
        Assert.NotNull(expr);
        Assert.Equal(123, result);
    }

    [Fact]
    public void CreateMemberExpression_PrivateField_ReturnsExpression()
    {
        // Arrange
        var type = typeof(MyClass);

        // Act
        var expr = type.CreateMemberExpression("_privateField");

        // Assert
        Assert.NotNull(expr);

        // Arrange
        var obj = new MyClass();

        // Act
        obj.SetPrivateField(77);
        var compiled = expr.Compile();
        var value = compiled.DynamicInvoke(obj);

        // Assert
        Assert.Equal(77, value);
    }

    [Fact]
    public void CreateMemberExpression_WithInvalidProperty_ShouldThrowArgumentException()
    {
        // Arrange
        var type = typeof(MyClass);
        const string invalidPropertyName = "InvalidProperty";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => type.CreateMemberExpression(invalidPropertyName));
    }

    [Fact]
    public void CreateMemberExpression_WithNullType_ShouldThrowArgumentNullException()
    {
        // Arrange
        Type type = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => type.CreateMemberExpression("PropertyName"));
    }

    [Theory]
    [InlineData("PropertyName")]
    [InlineData("AnotherProperty")]
    public void CreateMemberExpression_ValidProperty_ShouldNotThrowException(string propertyName)
    {
        // Arrange & Act
        Exception ex = Record.Exception(() => propertyName.CreateMemberExpression<MyClass>());

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void CreateMemberExpression_StronglyTypedProperty_ReturnsExpression()
    {
        // Arrange
        var obj = new MyClass { AnotherProperty = 99 };

        // Act
        var expr = "AnotherProperty".CreateMemberExpression<MyClass, int>();

        // Assert
        Assert.NotNull(expr);

        // Arrange & Act
        var compiled = expr.Compile();
        var value = compiled(obj);

        // Assert
        Assert.Equal(99, value);
    }

    [Fact]
    public void CreateMemberExpression_NullOrWhitespace_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((string?)null)!.CreateMemberExpression<MyClass, string>());
        Assert.Throws<ArgumentException>(() => "".CreateMemberExpression<MyClass, string>());
        Assert.Throws<ArgumentException>(() => "   ".CreateMemberExpression<MyClass, string>());
    }

    [Fact]
    public void CreateStaticMemberExpression_ValidStaticField_ReturnsCorrectExpression()
    {
        // Arrange
        MyClass.StaticField = 42;

        // Act
        var expr = typeof(MyClass).CreateStaticMemberExpression<int>("StaticField");
        var func = expr.Compile();
        var result = func();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void CreateStaticMemberExpression_InvalidMemberName_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            typeof(MyClass).CreateStaticMemberExpression<int>("NonExistentField"));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void CreateStaticMemberExpression_NullMemberName_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            typeof(MyClass).CreateStaticMemberExpression<int>(null!));
    }

    [Fact]
    public void CreateStaticMemberExpression_InstanceField_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            typeof(MyClass).CreateStaticMemberExpression<int>("AnotherProperty"));
    }

    [Fact]
    public void CreateStaticMemberExpression_WrongResultType_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            typeof(MyClass).CreateStaticMemberExpression<string>("StaticField"));
        Assert.Contains("cannot be assigned", ex.Message);
    }

    [Fact]
    public void CreateMemberExpression_NonExistentMember_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            "NonExistent".CreateMemberExpression<MyClass, string>());

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void CreateMemberExpression_StronglyTyped_InvalidProperty_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => "NotExist".CreateMemberExpression<MyClass, int>());
    }

    [Fact]
    public void CreateMemberExpression_PrivateField_ReturnsCorrectExpression()
    {
        // Arrange & Act
        var expr = "_privateField".CreateMemberExpression<MyClass, int>();

        // Assert
        Assert.NotNull(expr);

        // Arrange & Act
        var func = expr.Compile();
        var instance = new MyClass();
        instance.SetPrivateField(55);

        // Assert
        Assert.Equal(55, func(instance));
    }

    [Fact]
    public void CreateMemberExpression_StaticField_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            "StaticField".CreateMemberExpression<MyClass, int>());

        Assert.Contains("not found", ex.Message); // because static members are excluded
    }

    [Fact]
    public void CreateMemberExpression_TypeMismatch_ThrowsInvalidOperationException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            "AnotherProperty".CreateMemberExpression<MyClass, string>());

        Assert.Contains("cannot be assigned", ex.Message);
    }

    [Fact]
    public void And_ReturnsCombinedExpression()
    {
        // Arrange
        Expression<Func<MyClass, bool>> expr1 = x => x.AnotherProperty > 10;
        Expression<Func<MyClass, bool>> expr2 = x => x.PropertyName == "Test";

        // Act
        var andExpr = expr1.And(expr2);

        // Assert
        Assert.NotNull(andExpr);

        // Arrange
        var obj1 = new MyClass { AnotherProperty = 20, PropertyName = "Test" };
        var obj2 = new MyClass { AnotherProperty = 5, PropertyName = "Test" };

        // Act
        var compiled = andExpr.Compile();

        // Assert
        Assert.True(compiled(obj1));
        Assert.False(compiled(obj2));
    }

    [Fact]
    public void Or_ReturnsCombinedExpression()
    {
        // Arrange
        Expression<Func<MyClass, bool>> expr1 = x => x.AnotherProperty > 10;
        Expression<Func<MyClass, bool>> expr2 = x => x.PropertyName == "Test";

        // Act
        var orExpr = expr1.Or(expr2);

        // Assert
        Assert.NotNull(orExpr);

        // Arrange
        var obj1 = new MyClass { AnotherProperty = 20, PropertyName = "No" };
        var obj2 = new MyClass { AnotherProperty = 5, PropertyName = "Test" };
        var obj3 = new MyClass { AnotherProperty = 5, PropertyName = "No" };

        // Act
        var compiled = orExpr.Compile();

        // Assert
        Assert.True(compiled(obj1));
        Assert.True(compiled(obj2));
        Assert.False(compiled(obj3));
    }

    private class MyClass
    {
        public int AnotherProperty { get; set; }
        public string? PropertyName { get; set; }
        public static int StaticField;
        private int _privateField;
        public int GetPrivateField() => _privateField;
        public void SetPrivateField(int value) => _privateField = value;
    }
}
