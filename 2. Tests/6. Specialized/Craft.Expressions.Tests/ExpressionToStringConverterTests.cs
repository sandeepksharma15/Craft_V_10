using System.Linq.Expressions;
using Craft.Testing.Fixtures;

namespace Craft.Expressions.Tests;

/// <summary>
/// Unit tests for ExpressionToStringConverter covering all supported expression types and edge cases.
/// </summary>
public class ExpressionToStringConverterTests
{
    private static readonly ParameterExpression StoreParam = Expression.Parameter(typeof(Store), "s");
    private static readonly ParameterExpression CompanyParam = Expression.Parameter(typeof(Company), "c");
    private static readonly ParameterExpression TestEntityParam = Expression.Parameter(typeof(TestEntity), "e");
    private static readonly ParameterExpression TestUserParam = Expression.Parameter(typeof(TestUser), "u");

    [Fact]
    public void Convert_BinaryExpression_Equal()
    {
        // Arrange
        var expr = Expression.Equal(
            Expression.Property(TestEntityParam, nameof(TestEntity.Name)),
            Expression.Constant("John")
        );

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("(Name == \"John\")", result);
    }

    [Fact]
    public void Convert_BinaryExpression_NotEqual()
    {
        // Arrange
        var expr = Expression.NotEqual(
            Expression.Property(StoreParam, nameof(Store.City)),
            Expression.Constant("London")
        );

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("(City != \"London\")", result);
    }

    [Fact]
    public void Convert_BinaryExpression_GreaterThan()
    {
        // Arrange
        var expr = Expression.GreaterThan(
            Expression.Property(CompanyParam, nameof(Company.CountryId)),
            Expression.Constant(10L) // Use long to match CountryId type
        );

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("(CountryId > 10)", result);
    }

    [Fact]
    public void Convert_BinaryExpression_LessThanOrEqual()
    {
        // Arrange
        var expr = Expression.LessThanOrEqual(
            Expression.Property(StoreParam, nameof(Store.CompanyId)),
            Expression.Constant(5L)
        );

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("(CompanyId <= 5)", result);
    }

    [Fact]
    public void Convert_UnaryExpression_Not()
    {
        // Arrange
        var expr = Expression.Not(
            Expression.Property(TestUserParam, nameof(TestUser.EmailConfirmed))
        );

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("!EmailConfirmed", result);
    }

    [Fact]
    public void Convert_BinaryExpression_AndAlso()
    {
        // Arrange
        var expr = Expression.AndAlso(
            Expression.Property(TestUserParam, nameof(TestUser.EmailConfirmed)),
            Expression.Equal(Expression.Property(TestUserParam, nameof(TestUser.UserName)), Expression.Constant("admin"))
        );

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("(EmailConfirmed && (UserName == \"admin\"))", result);
    }

    [Fact]
    public void Convert_BinaryExpression_OrElse()
    {
        // Arrange
        var expr = Expression.OrElse(
            Expression.Property(TestUserParam, nameof(TestUser.EmailConfirmed)),
            Expression.Property(TestUserParam, nameof(TestUser.LockoutEnabled))
        );

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("(EmailConfirmed || LockoutEnabled)", result);
    }

    [Fact]
    public void Convert_NestedMemberExpression()
    {
        // Arrange
        // Store.Company.Name == "Acme"
        var companyExpr = Expression.Property(StoreParam, nameof(Store.Company));
        var nameExpr = Expression.Property(companyExpr, nameof(Company.Name));
        var expr = Expression.Equal(nameExpr, Expression.Constant("Acme"));

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("(Company.Name == \"Acme\")", result);
    }

    [Fact]
    public void Convert_ConstantExpression_String()
    {
        // Arrange
        var expr = Expression.Constant("Hello");

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("\"Hello\"", result);
    }

    [Fact]
    public void Convert_ConstantExpression_Bool()
    {
        // Arrange
        var expr = Expression.Constant(true);

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("true", result);
    }

    [Fact]
    public void Convert_ConstantExpression_Null()
    {
        // Arrange
        var expr = Expression.Constant(null, typeof(string));

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("null", result);
    }

    [Fact]
    public void Convert_ConstantExpression_Char()
    {
        // Arrange
        var expr = Expression.Constant('A');

        // Act
        var result = ExpressionToStringConverter.Convert(expr);

        // Assert
        Assert.Equal("'A'", result);
    }

    [Fact]
    public void Convert_MethodCall_Contains()
    {
        // Arrange
        // e.Name.Contains("oh")
        var nameExpr = Expression.Property(TestEntityParam, nameof(TestEntity.Name));
        var containsExpr = Expression.Call(
            nameExpr,
            typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!,
            Expression.Constant("oh")
        );

        // Act
        var result = ExpressionToStringConverter.Convert(containsExpr);

        // Assert
        Assert.Equal("Name.Contains(\"oh\")", result);
    }

    [Fact]
    public void Convert_MethodCall_StartsWith()
    {
        // Arrange
        var nameExpr = Expression.Property(TestEntityParam, nameof(TestEntity.Name));
        var startsWithExpr = Expression.Call(
            nameExpr,
            typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!,
            Expression.Constant("J")
        );

        // Act
        var result = ExpressionToStringConverter.Convert(startsWithExpr);

        // Assert
        Assert.Equal("Name.StartsWith(\"J\")", result);
    }

    [Fact]
    public void Convert_MethodCall_EndsWith()
    {
        // Arrange
        var nameExpr = Expression.Property(TestEntityParam, nameof(TestEntity.Name));
        var endsWithExpr = Expression.Call(
            nameExpr,
            typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!,
            Expression.Constant("n")
        );

        // Act
        var result = ExpressionToStringConverter.Convert(endsWithExpr);

        // Assert
        Assert.Equal("Name.EndsWith(\"n\")", result);
    }

    [Fact]
    public void Convert_UnsupportedExpression_Throws()
    {
        // Arrange
        var lambda = Expression.Lambda(Expression.Block());

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => ExpressionToStringConverter.Convert(lambda.Body));
        Assert.Contains("Expression type", ex.Message);
    }

    [Fact]
    public void Convert_MemberExpression_UnsupportedParent_Throws()
    {
        // Arrange
        // Member access on a constant (not parameter or member)
        var constExpr = Expression.Constant(new TestEntity());
        var memberExpr = Expression.Property(constExpr, nameof(TestEntity.Name));

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => ExpressionToStringConverter.Convert(memberExpr));
        Assert.Contains("Only member access on the parameter or its properties is supported", ex.Message);
    }

    [Fact]
    public void Convert_BinaryExpression_Equal_DirectLambda()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr = e => e.Name == "John";

        // Act
        var result = ExpressionToStringConverter.Convert(expr.Body);

        // Assert
        Assert.Equal("(Name == \"John\")", result);
    }

    [Fact]
    public void Convert_BinaryExpression_AndAlso_DirectLambda()
    {
        // Arrange
        Expression<Func<TestUser, bool>> expr = u => u.EmailConfirmed && u.UserName == "admin";

        // Act
        var result = ExpressionToStringConverter.Convert(expr.Body);

        // Assert
        Assert.Equal("(EmailConfirmed && (UserName == \"admin\"))", result);
    }

    [Fact]
    public void Convert_MethodCall_Contains_DirectLambda()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr = e => e.Name.Contains("oh");

        // Act
        var result = ExpressionToStringConverter.Convert(expr.Body);

        // Assert
        Assert.Equal("Name.Contains(\"oh\")", result);
    }

    [Fact]
    public void Convert_NestedMemberExpression_DirectLambda()
    {
        // Arrange
        Expression<Func<Store, bool>> expr = s => s.Company!.Name == "Acme";

        // Act
        var result = ExpressionToStringConverter.Convert(expr.Body);

        // Assert
        Assert.Equal("(Company.Name == \"Acme\")", result);
    }
}
