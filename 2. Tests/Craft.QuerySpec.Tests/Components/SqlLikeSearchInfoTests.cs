using System.Linq.Expressions;

namespace Craft.QuerySpec.Tests.Components;

public class SqlLikeSearchInfoTests
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void Ctor_ExpressionOverload_SetsProperties()
    {
        Expression<Func<TestEntity, object>> expr = x => x.Name;
        var info = new SqlLikeSearchInfo<TestEntity>(expr, "abc", 2);
        Assert.Equal(expr, info.SearchItem);
        Assert.Equal("abc", info.SearchString);
        Assert.Equal(2, info.SearchGroup);
    }

    [Fact]
    public void Ctor_ExpressionOverload_DefaultGroup_SetsGroupTo1()
    {
        Expression<Func<TestEntity, object>> expr = x => x.Age;
        var info = new SqlLikeSearchInfo<TestEntity>(expr, "42");
        Assert.Equal(expr, info.SearchItem);
        Assert.Equal("42", info.SearchString);
        Assert.Equal(1, info.SearchGroup);
    }

    [Fact]
    public void Ctor_LambdaExpressionOverload_SetsProperties()
    {
        LambdaExpression expr = (Expression<Func<TestEntity, object>>)(x => x.Name);
        var info = new SqlLikeSearchInfo<TestEntity>(expr, "xyz", 3);
        Assert.Equal(expr, info.SearchItem);
        Assert.Equal("xyz", info.SearchString);
        Assert.Equal(3, info.SearchGroup);
    }

    [Fact]
    public void Ctor_LambdaExpressionOverload_DefaultGroup_SetsGroupTo1()
    {
        LambdaExpression expr = (Expression<Func<TestEntity, object>>)(x => x.Age);
        var info = new SqlLikeSearchInfo<TestEntity>(expr, "test");
        Assert.Equal(expr, info.SearchItem);
        Assert.Equal("test", info.SearchString);
        Assert.Equal(1, info.SearchGroup);
    }

    [Fact]
    public void Ctor_ExpressionOverload_NullExpression_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlLikeSearchInfo<TestEntity>(null!, "abc"));
    }

    [Fact]
    public void Ctor_LambdaExpressionOverload_NullExpression_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlLikeSearchInfo<TestEntity>((LambdaExpression)null!, "abc"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Ctor_ExpressionOverload_NullOrWhitespaceSearchString_Throws(string? search)
    {
        Expression<Func<TestEntity, object>> expr = x => x.Name;

        if (search is null)
            Assert.Throws<ArgumentNullException>(() => new SqlLikeSearchInfo<TestEntity>(expr, search!));
        else
            Assert.Throws<ArgumentException>(() => new SqlLikeSearchInfo<TestEntity>(expr, search!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Ctor_LambdaExpressionOverload_NullOrWhitespaceSearchString_Throws(string? search)
    {
        LambdaExpression expr = (Expression<Func<TestEntity, object>>)(x => x.Name);

        if (search is null)
            Assert.Throws<ArgumentNullException>(() => new SqlLikeSearchInfo<TestEntity>(expr, search!));
        else
            Assert.Throws<ArgumentException>(() => new SqlLikeSearchInfo<TestEntity>(expr, search!));
    }

    [Fact]
    public void InternalParameterlessCtor_CanInstantiate()
    {
        var ctor = typeof(SqlLikeSearchInfo<TestEntity>).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, Type.EmptyTypes, null);
        var instance = ctor!.Invoke(null);
        Assert.NotNull(instance);
    }
}
