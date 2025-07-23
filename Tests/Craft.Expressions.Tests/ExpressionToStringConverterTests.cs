using System.Linq.Expressions;
using Craft.TestDataStore.Models;

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
        var expr = Expression.Equal(
            Expression.Property(TestEntityParam, nameof(TestEntity.Name)),
            Expression.Constant("John")
        );
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("(Name == \"John\")", result);
    }

    [Fact]
    public void Convert_BinaryExpression_NotEqual()
    {
        var expr = Expression.NotEqual(
            Expression.Property(StoreParam, nameof(Store.City)),
            Expression.Constant("London")
        );
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("(City != \"London\")", result);
    }

    [Fact]
    public void Convert_BinaryExpression_GreaterThan()
    {
        var expr = Expression.GreaterThan(
            Expression.Property(CompanyParam, nameof(Company.CountryId)),
            Expression.Constant(10L) // Use long to match CountryId type
        );
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("(CountryId > 10)", result);
    }

    [Fact]
    public void Convert_BinaryExpression_LessThanOrEqual()
    {
        var expr = Expression.LessThanOrEqual(
            Expression.Property(StoreParam, nameof(Store.CompanyId)),
            Expression.Constant(5L)
        );
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("(CompanyId <= 5)", result);
    }

    [Fact]
    public void Convert_UnaryExpression_Not()
    {
        var expr = Expression.Not(
            Expression.Property(TestUserParam, nameof(TestUser.EmailConfirmed))
        );
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("!EmailConfirmed", result);
    }

    [Fact]
    public void Convert_BinaryExpression_AndAlso()
    {
        var expr = Expression.AndAlso(
            Expression.Property(TestUserParam, nameof(TestUser.EmailConfirmed)),
            Expression.Equal(Expression.Property(TestUserParam, nameof(TestUser.UserName)), Expression.Constant("admin"))
        );
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("(EmailConfirmed && (UserName == \"admin\"))", result);
    }

    [Fact]
    public void Convert_BinaryExpression_OrElse()
    {
        // Use a valid boolean OrElse expression
        var expr = Expression.OrElse(
            Expression.Property(TestUserParam, nameof(TestUser.EmailConfirmed)),
            Expression.Property(TestUserParam, nameof(TestUser.LockoutEnabled))
        );
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("(EmailConfirmed || LockoutEnabled)", result);
    }

    [Fact]
    public void Convert_NestedMemberExpression()
    {
        // Store.Company.Name == "Acme"
        var companyExpr = Expression.Property(StoreParam, nameof(Store.Company));
        var nameExpr = Expression.Property(companyExpr, nameof(Company.Name));
        var expr = Expression.Equal(nameExpr, Expression.Constant("Acme"));
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("(Company.Name == \"Acme\")", result);
    }

    [Fact]
    public void Convert_ConstantExpression_String()
    {
        var expr = Expression.Constant("Hello");
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("\"Hello\"", result);
    }

    [Fact]
    public void Convert_ConstantExpression_Bool()
    {
        var expr = Expression.Constant(true);
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("true", result);
    }

    [Fact]
    public void Convert_ConstantExpression_Null()
    {
        var expr = Expression.Constant(null, typeof(string));
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("null", result);
    }

    [Fact]
    public void Convert_ConstantExpression_Char()
    {
        var expr = Expression.Constant('A');
        var result = ExpressionToStringConverter.Convert(expr);
        Assert.Equal("'A'", result);
    }

    [Fact]
    public void Convert_MethodCall_Contains()
    {
        // e.Name.Contains("oh")
        var nameExpr = Expression.Property(TestEntityParam, nameof(TestEntity.Name));
        var containsExpr = Expression.Call(
            nameExpr,
            typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
            Expression.Constant("oh")
        );
        var result = ExpressionToStringConverter.Convert(containsExpr);
        Assert.Equal("Name.Contains(\"oh\")", result);
    }

    [Fact]
    public void Convert_MethodCall_StartsWith()
    {
        var nameExpr = Expression.Property(TestEntityParam, nameof(TestEntity.Name));
        var startsWithExpr = Expression.Call(
            nameExpr,
            typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!,
            Expression.Constant("J")
        );
        var result = ExpressionToStringConverter.Convert(startsWithExpr);
        Assert.Equal("Name.StartsWith(\"J\")", result);
    }

    [Fact]
    public void Convert_MethodCall_EndsWith()
    {
        var nameExpr = Expression.Property(TestEntityParam, nameof(TestEntity.Name));
        var endsWithExpr = Expression.Call(
            nameExpr,
            typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!,
            Expression.Constant("n")
        );
        var result = ExpressionToStringConverter.Convert(endsWithExpr);
        Assert.Equal("Name.EndsWith(\"n\")", result);
    }

    [Fact]
    public void Convert_UnsupportedExpression_Throws()
    {
        var lambda = Expression.Lambda(Expression.Block());
        var ex = Assert.Throws<NotSupportedException>(() => ExpressionToStringConverter.Convert(lambda.Body));
        Assert.Contains("Expression type", ex.Message);
    }

    [Fact]
    public void Convert_MemberExpression_UnsupportedParent_Throws()
    {
        // Member access on a constant (not parameter or member)
        var constExpr = Expression.Constant(new TestEntity());
        var memberExpr = Expression.Property(constExpr, nameof(TestEntity.Name));
        var ex = Assert.Throws<NotSupportedException>(() => ExpressionToStringConverter.Convert(memberExpr));
        Assert.Contains("Only member access on the parameter or its properties is supported", ex.Message);
    }
}
