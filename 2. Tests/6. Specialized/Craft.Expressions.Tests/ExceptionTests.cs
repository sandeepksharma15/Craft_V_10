namespace Craft.Expressions.Tests;

public class ExceptionTests
{
    private static ExpressionSerializer<TestClass> Serializer => new();

    private class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void ExpressionTokenizationException_ContainsPositionAndCharacter()
    {
        // Arrange
        var invalidExpression = "Name $ \"John\"";

        // Act
        var ex = Assert.Throws<ExpressionTokenizationException>(() =>
            Serializer.Deserialize(invalidExpression));

        // Assert
        Assert.Equal(5, ex.Position);
        Assert.Equal('$', ex.Character);
        Assert.Contains("position 5", ex.Message);
        Assert.Contains("'$'", ex.Message);
    }

    [Fact]
    public void ExpressionTokenizationException_ThrowsForSingleEquals()
    {
        // Arrange
        var invalidExpression = "Name = \"John\"";

        // Act
        var ex = Assert.Throws<ExpressionTokenizationException>(() =>
            Serializer.Deserialize(invalidExpression));

        // Assert
        Assert.Equal(5, ex.Position);
        Assert.Equal('=', ex.Character);
        Assert.Contains("did you mean '=='", ex.Message);
    }

    [Fact]
    public void ExpressionTokenizationException_ThrowsForSingleAmpersand()
    {
        // Arrange
        var invalidExpression = "Age > 18 & Name != \"John\"";

        // Act
        var ex = Assert.Throws<ExpressionTokenizationException>(() =>
            Serializer.Deserialize(invalidExpression));

        // Assert
        Assert.Equal('&', ex.Character);
        Assert.Contains("did you mean '&&'", ex.Message);
    }

    [Fact]
    public void ExpressionTokenizationException_ThrowsForSinglePipe()
    {
        // Arrange
        var invalidExpression = "Age > 18 | Name != \"John\"";

        // Act
        var ex = Assert.Throws<ExpressionTokenizationException>(() =>
            Serializer.Deserialize(invalidExpression));

        // Assert
        Assert.Equal('|', ex.Character);
        Assert.Contains("did you mean '||'", ex.Message);
    }

    [Fact]
    public void ExpressionParseException_ContainsPositionAndToken()
    {
        // Arrange
        var invalidExpression = "Name ==";

        // Act
        var ex = Assert.Throws<ExpressionParseException>(() =>
            Serializer.Deserialize(invalidExpression));

        // Assert
        Assert.True(ex.Position >= 0);
        Assert.NotNull(ex.Token);
        Assert.Contains("position", ex.Message);
    }

    [Fact]
    public void ExpressionParseException_ThrowsForMissingClosingParen()
    {
        // Arrange
        var invalidExpression = "(Name == \"John\"";

        // Act
        var ex = Assert.Throws<ExpressionParseException>(() =>
            Serializer.Deserialize(invalidExpression));

        // Assert
        Assert.Contains("Expected ')'", ex.Message);
    }

    [Fact]
    public void ExpressionParseException_ThrowsForUnexpectedToken()
    {
        // Arrange
        var invalidExpression = "Name == \"John\" extra";

        // Act
        var ex = Assert.Throws<ExpressionParseException>(() =>
            Serializer.Deserialize(invalidExpression));

        // Assert
        Assert.Contains("Unexpected token", ex.Message);
        Assert.Equal("extra", ex.Token);
    }

    [Fact]
    public void ExpressionEvaluationException_ContainsMemberPathAndType()
    {
        // Arrange
        var invalidExpression = "NonExistentProperty == \"test\"";

        // Act
        var ex = Assert.Throws<ExpressionEvaluationException>(() =>
            Serializer.Deserialize(invalidExpression));

        // Assert
        Assert.Equal(typeof(TestClass), ex.TargetType);
        Assert.Equal("NonExistentProperty", ex.MemberPath);
        Assert.Contains("NonExistentProperty", ex.Message);
        Assert.Contains("TestClass", ex.Message);
    }

    [Fact]
    public void ExpressionEvaluationException_ThrowsForNonExistentMethod()
    {
        // Arrange
        var invalidExpression = "Name.NonExistentMethod()";

        // Act
        var ex = Assert.Throws<ExpressionEvaluationException>(() =>
            Serializer.Deserialize(invalidExpression));

        // Assert
        Assert.Contains("NonExistentMethod", ex.Message);
    }

    [Fact]
    public void NotSupportedException_ThrowsForUnsupportedOperator()
    {
        // Arrange - This would require modifying parser to allow unsupported operators
        // For now, we test that operators are validated during tokenization
        var invalidExpression = "Age % 2 == 0";

        // Act & Assert
        Assert.Throws<ExpressionTokenizationException>(() =>
            Serializer.Deserialize(invalidExpression));
    }
}
