using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Craft.QuerySpec;

namespace Craft.QuerySpec.Tests.Builders;

public class QuerySelectBuilderTests
{
    public class Source
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }
    public class Dest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void Ctor_Initializes_EmptyList_And_CountIsZero()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Assert.NotNull(builder.SelectDescriptorList);
        Assert.Empty(builder.SelectDescriptorList);
        Assert.Equal(0, builder.Count);
    }

    [Fact]
    public void Add_ByPropertyName_AddsDescriptor_And_ReturnsSelf()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        var result = builder.Add("Name");
        Assert.Single(builder.SelectDescriptorList);
        Assert.Equal(builder, result);
    }

    [Fact]
    public void Add_ByPropertyName_ThrowsOnNull()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((string)null!));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneePropertyName_AddsDescriptor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add("Name", "Name");
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByAssignorAndAssigneePropertyName_ThrowsOnNullAssignor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Assert.Throws<ArgumentNullException>(() => builder.Add(null!, "Name"));
    }

    [Fact]
    public void Add_ByExpression_AddsDescriptor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add(x => x.Name);
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByExpression_ThrowsOnNull()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<Source, object>>)null!));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeExpression_AddsDescriptor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add(x => x.Name, x => x.Name);
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeExpression_ThrowsOnNullAssignor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Assert.Throws<ArgumentNullException>(() => builder.Add(null!, x => x.Name));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeExpression_ThrowsOnNullAssignee()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Assert.Throws<ArgumentNullException>(() => builder.Add(x => x.Name, null!));
    }

    [Fact]
    public void Add_BySelectDescriptor_AddsDescriptor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        var desc = new SelectDescriptor<Source, Dest>("Name");
        builder.Add(desc);
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_BySelectDescriptor_ThrowsOnNull()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((SelectDescriptor<Source, Dest>)null!));
    }

    [Fact]
    public void Add_ByLambdaExpressions_AddsDescriptor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Expression<Func<Source, object>> assignor = x => x.Name;
        Expression<Func<Dest, object>> assignee = x => x.Name;
        builder.Add(assignor, assignee);
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByLambdaExpressions_ThrowsOnNullAssignor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Expression<Func<Dest, object>> assignee = x => x.Name;
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<Source, object>>)null!, assignee));
    }

    [Fact]
    public void Add_ByLambdaExpressions_ThrowsOnNullAssignee()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Expression<Func<Source, object>> assignor = x => x.Name;
        Assert.Throws<ArgumentNullException>(() => builder.Add(assignor, (Expression<Func<Dest, object>>)null!));
    }

    [Fact]
    public void Add_ByLambdaExpression_AddsDescriptor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Expression<Func<Source, object>> column = x => x.Name;
        builder.Add((LambdaExpression)column);
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByLambdaExpression_ThrowsOnNull()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((LambdaExpression)null!));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeLambda_AddsDescriptor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Expression<Func<Source, object>> assignor = x => x.Name;
        Expression<Func<Dest, object>> assignee = x => x.Name;
        builder.Add((LambdaExpression)assignor, (LambdaExpression)assignee);
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeLambda_ThrowsOnNullAssignor()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Expression<Func<Dest, object>> assignee = x => x.Name;
        Assert.Throws<ArgumentNullException>(() => builder.Add((LambdaExpression)null!, (LambdaExpression)assignee));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeLambda_ThrowsOnNullAssignee()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Expression<Func<Source, object>> assignor = x => x.Name;
        Assert.Throws<ArgumentNullException>(() => builder.Add((LambdaExpression)assignor, (LambdaExpression)null!));
    }

    [Fact]
    public void Count_ReflectsAddedItems()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add("Name");
        builder.Add("Age");
        Assert.Equal(2, builder.Count);
    }

    [Fact]
    public void Clear_EmptiesList()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add("Name");
        builder.Add("Age");
        builder.Clear();
        Assert.Empty(builder.SelectDescriptorList);
        Assert.Equal(0, builder.Count);
    }

    [Fact]
    public void Build_ThrowsIfNoMappings_NonAnonymous()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_ReturnsNullIfNoMappings_Anonymous()
    {
        var builder = new QuerySelectBuilder<Source, object>();
        Assert.Null(builder.Build());
    }

    [Fact]
    public void Build_ReturnsValidExpression_NonAnonymous()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add("Name", "Name");
        var expr = builder.Build();
        Assert.NotNull(expr);
        var compiled = expr!.Compile();
        var result = compiled(new Source { Name = "Test" });
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public void Build_ReturnsValidExpression_Anonymous()
    {
        var builder = new QuerySelectBuilder<Source, object>();
        builder.Add(x => x.Name);
        var expr = builder.Build();
        Assert.NotNull(expr);
        var compiled = expr!.Compile();
        var arr = compiled(new Source { Name = "Test" }) as object[];
        Assert.NotNull(arr);
        Assert.Equal("Test", arr![0]);
    }

    [Fact]
    public void Build_ThrowsIfAssignorIsNull_NonAnonymous()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        var desc = (SelectDescriptor<Source, Dest>)Activator.CreateInstance(typeof(SelectDescriptor<Source, Dest>), true)!;
        builder.Add(desc);
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_ThrowsIfAssignorIsNull_Anonymous()
    {
        var builder = new QuerySelectBuilder<Source, object>();
        var desc = (SelectDescriptor<Source, object>)Activator.CreateInstance(typeof(SelectDescriptor<Source, object>), true)!;
        builder.Add(desc);
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Add_DuplicateColumns_AllowsDuplicates()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add("Name");
        builder.Add("Name");
        Assert.Equal(2, builder.Count);
    }

    [Fact]
    public void Add_MultipleColumns_And_Build_Works()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add("Name", "Name");
        builder.Add("Age", "Age");
        var expr = builder.Build();
        Assert.NotNull(expr);
        var compiled = expr!.Compile();
        var result = compiled(new Source { Name = "A", Age = 42 });
        Assert.Equal("A", result.Name);
        Assert.Equal(42, result.Age);
    }

    [Fact]
    public void QuerySelectBuilderT_IsAssignableToQuerySelectBuilderTT()
    {
        var builder = new QuerySelectBuilder<Source>();
        Assert.IsAssignableFrom<QuerySelectBuilder<Source, Source>>(builder);
        builder.Add("Name");
        Assert.Single(builder.SelectDescriptorList);
    }
}
