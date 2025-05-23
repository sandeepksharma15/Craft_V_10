namespace Craft.Extensions.Tests.System;

public class ObjectTests
{
    [Fact]
    public void AsType_Should_Return_Correct_Type()
    {
        // Arrange
        object obj = "Hello, World!";

        // Act
        var result = obj.AsType<string>();

        // Assert
        Assert.IsType<string>(result);
        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void AsType_Should_Return_Null_For_Incorrect_Type()
    {
        // Arrange
        object obj = 123;

        // Act & Assert
        Assert.Throws<InvalidCastException>(() => obj.AsType<string>());
    }

    [Fact]
    public void AsType_Should_Return_Null_When_Object_Is_Null()
    {
        // Arrange
        object obj = null!;

        // Act
        var result = obj.AsType<string>();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void If_Should_Apply_Function_When_Condition_Is_True()
    {
        // Arrange
        const int value = 5;
        const bool condition = true;

        // Act
        var result = value.If(condition, x => x * 2);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void If_Should_Not_Apply_Function_When_Condition_Is_False()
    {
        // Arrange
        const int value = 5;
        const bool condition = false;

        // Act
        var result = value.If(condition, x => x * 2);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void If_Should_Not_Perform_Action_When_Action_Is_Null()
    {
        // Arrange
        const int value = 5;
        const bool condition = true;

        // Act
        int newValue = value.If(condition, null!);

        // Assert
        Assert.Equal(value, newValue);
    }

    [Fact]
    public void If_Should_Not_Perform_Action_When_Condition_Is_False()
    {
        // Arrange
        const int value = 5;
        const bool condition = false;
        int newValue = 0;

        // Act
        value.If(condition, x => newValue = x * 2);

        // Assert
        Assert.Equal(0, newValue);
    }

    [Fact]
    public void If_Should_Perform_Action_When_Condition_Is_True()
    {
        // Arrange
        const int value = 5;
        const bool condition = true;
        int newValue = 0;

        // Act
        value.If(condition, x => newValue = x * 2);

        // Assert
        Assert.Equal(10, newValue);
    }

    [Fact]
    public void If_Should_Return_Same_Value_When_Condition_Is_False_And_No_Action_Provided()
    {
        // Arrange
        const string text = "Hello";
        const bool condition = false;

        // Act
        var result = text.If(condition, null!);

        // Assert
        Assert.Equal(text, result);
    }

    [Fact]
    public void If_Should_Return_Same_Value_When_Condition_Is_True_And_No_Function_Provided()
    {
        // Arrange
        const string text = "Hello";
        const bool condition = true;

        // Act
        var result = text.If(condition, null!);

        // Assert
        Assert.Equal(text, result);
    }

    [Fact]
    public void If_WithFunc_Should_Handle_Null_Object()
    {
        // Arrange
        string? obj = null;

        // Act
        var result = obj.If(true, s => s ?? "default");

        // Assert
        Assert.Equal("default", result);
    }

    [Fact]
    public void If_WithAction_Should_Handle_Null_Object()
    {
        // Arrange
        string? obj = null;
        bool called = false;

        // Act
        var result = obj.If(true, s => { if (s == null) called = true; });

        // Assert
        Assert.True(called);
        Assert.Null(result);
    }

    [Fact]
    public void ToValue_Should_Convert_Guid_Correctly()
    {
        // Arrange
        object obj = "6F9619FF-8B86-D011-B42D-00C04FC964FF";

        // Act
        var result = obj.ToValue<Guid>();

        // Assert
        Assert.Equal(Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF"), result);
    }

    [Fact]
    public void ToValue_Should_Convert_To_Correct_Type()
    {
        // Arrange
        object obj = "123";

        // Act
        var result = obj.ToValue<int>();

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void ToValue_Should_Throw_Exception_For_Invalid_Conversion()
    {
        // Arrange
        object obj = "InvalidNumber";

        // Act
        var action = new Action(() => obj.ToValue<int>());

        // Act & Assert
        Assert.Throws<FormatException>(() => obj.ToValue<int>());
    }

    [Fact]
    public void ToValue_Should_Return_Default_For_Null()
    {
        // Arrange
        object obj = null!;

        // Act
        var result = obj.ToValue<int>();

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void ToValue_Should_Return_Default_For_Failed_Guid_Parse()
    {
        // Arrange
        object obj = "not-a-guid";

        // Act
        var result = obj.ToValue<Guid>();

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void ToValue_Should_Return_Default_For_NonConvertible_Type()
    {
        // Arrange
        object obj = new ObjectTests();

        // Act
        var result = obj.ToValue<int>();

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void ToValue_Should_Convert_Boxed_Value_Type()
    {
        // Arrange
        object obj = (object)42;

        // Act
        var result = obj.ToValue<int>();

        // Assert
        Assert.Equal(42, result);
    }
}
