using System.Linq.Expressions;

namespace Craft.QuerySpec.Tests.Builders;

/// <summary>
/// Unit tests for QuerySelectBuilder core logic (excluding serialization).
/// </summary>
public class QuerySelectBuilderTests
{
    // Use a single set of test classes for all tests
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private class TestResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    #region Constructor & Basic Properties

    [Fact]
    public void Ctor_Initializes_EmptyList_And_CountIsZero()
    {
        // Arrange & Act
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Assert
        Assert.NotNull(builder.SelectDescriptorList);
        Assert.Empty(builder.SelectDescriptorList);
        Assert.Equal(0, builder.Count);
    }

    #endregion

    #region Add Method - Valid Cases

    [Fact]
    public void Add_ByPropertyName_AddsDescriptor_And_ReturnsSelf()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        var result = builder.Add("Name");
        // Assert
        Assert.Single(builder.SelectDescriptorList);
        Assert.Equal(builder, result);
    }

    [Fact]
    public void Add_ByAssignorAndAssigneePropertyName_AddsDescriptor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add("Name", "Name");
        // Assert
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByExpression_AddsDescriptor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add(x => x.Name);
        // Assert
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeExpression_AddsDescriptor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add(x => x.Name, x => x.Name);
        // Assert
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_BySelectDescriptor_AddsDescriptor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        var desc = new SelectDescriptor<TestEntity, TestResult>("Name");
        // Act
        builder.Add(desc);
        // Assert
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByLambdaExpressions_AddsDescriptor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        Expression<Func<TestEntity, object>> assignor = x => x.Name;
        Expression<Func<TestResult, object>> assignee = x => x.Name;
        // Act
        builder.Add(assignor, assignee);
        // Assert
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByLambdaExpression_AddsDescriptor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        Expression<Func<TestEntity, object>> column = x => x.Name;
        // Act
        builder.Add((LambdaExpression)column);
        // Assert
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeLambda_AddsDescriptor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        Expression<Func<TestEntity, object>> assignor = x => x.Name;
        Expression<Func<TestResult, object>> assignee = x => x.Name;
        // Act
        builder.Add(assignor, (LambdaExpression)assignee);
        // Assert
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_DuplicateColumns_AllowsDuplicates()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add("Name");
        builder.Add("Name");
        // Assert
        Assert.Equal(2, builder.Count);
    }

    #endregion

    #region Add Method - Invalid/Edge Cases

    [Theory]
    [InlineData(null)]
    public void Add_ByPropertyName_ThrowsOnNull(string? propertyName)
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(propertyName!));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneePropertyName_ThrowsOnNullAssignor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(null!, "Name"));
    }

    [Fact]
    public void Add_ByExpression_ThrowsOnNull()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<TestEntity, object>>)null!));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeExpression_ThrowsOnNullAssignor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(null!, x => x.Name));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeExpression_ThrowsOnNullAssignee()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(x => x.Name, null!));
    }

    [Fact]
    public void Add_BySelectDescriptor_ThrowsOnNull()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((SelectDescriptor<TestEntity, TestResult>)null!));
    }

    [Fact]
    public void Add_ByLambdaExpressions_ThrowsOnNullAssignor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        Expression<Func<TestResult, object>> assignee = x => x.Name;
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(null!, assignee));
    }

    [Fact]
    public void Add_ByLambdaExpressions_ThrowsOnNullAssignee()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        Expression<Func<TestEntity, object>> assignor = x => x.Name;
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(assignor, null!));
    }

    [Fact]
    public void Add_ByLambdaExpression_ThrowsOnNull()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((LambdaExpression)null!));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeLambda_ThrowsOnNullAssignor()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        Expression<Func<TestResult, object>> assignee = x => x.Name;
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(null!, (LambdaExpression)assignee));
    }

    [Fact]
    public void Add_ByAssignorAndAssigneeLambda_ThrowsOnNullAssignee()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        Expression<Func<TestEntity, object>> assignor = x => x.Name;
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(assignor, (LambdaExpression)null!));
    }

    [Fact]
    public void Add_InvalidPropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add("NotAProp", "Name"));
        Assert.Throws<ArgumentException>(() => builder.Add("Name", "NotAProp"));
    }

    #endregion

    #region Count & Clear

    [Fact]
    public void Count_ReflectsAddedItems()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add("Name");
        builder.Add("Age");
        // Assert
        Assert.Equal(2, builder.Count);
    }

    [Fact]
    public void Clear_EmptiesList()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        builder.Add("Name");
        builder.Add("Age");
        // Act
        builder.Clear();
        // Assert
        Assert.Empty(builder.SelectDescriptorList);
        Assert.Equal(0, builder.Count);
    }

    [Fact]
    public void Clear_AfterAddingColumns_EmptiesList()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        builder.Add(s => s.Name, d => d.Name);
        // Act
        builder.Clear();
        // Assert
        Assert.Empty(builder.SelectDescriptorList);
    }

    #endregion

    #region Build Method

    [Fact]
    public void Build_ThrowsIfNoMappings_NonAnonymous()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_ReturnsNullIfNoMappings_Anonymous()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, object>();
        // Act & Assert
        Assert.Null(builder.Build());
    }

    [Fact]
    public void Build_ReturnsValidExpression_NonAnonymous()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        builder.Add("Name", "Name");
        // Act
        var expr = builder.Build();
        // Assert
        Assert.NotNull(expr);
        var compiled = expr!.Compile();
        var result = compiled(new TestEntity { Name = "Test" });
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public void Build_ReturnsValidExpression_Anonymous()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, object>();
        builder.Add(x => x.Name);
        // Act
        var expr = builder.Build();
        // Assert
        Assert.NotNull(expr);
        var compiled = expr!.Compile();
        var arr = compiled(new TestEntity { Name = "Test" }) as object[];
        Assert.NotNull(arr);
        Assert.Equal("Test", arr![0]);
    }

    [Fact]
    public void Build_ThrowsIfAssignorIsNull_NonAnonymous()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        var desc = (SelectDescriptor<TestEntity, TestResult>)Activator.CreateInstance(typeof(SelectDescriptor<TestEntity, TestResult>), true)!;
        builder.Add(desc);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_ThrowsIfAssignorIsNull_Anonymous()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, object>();
        var desc = (SelectDescriptor<TestEntity, object>)Activator.CreateInstance(typeof(SelectDescriptor<TestEntity, object>), true)!;
        builder.Add(desc);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_Should_Create_SelectPredicate_When_TResult_Equals_T()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestEntity>();
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_Should_Create_SelectPredicateForResult_When_TResult_Not_T()
    {
        // Arrange
        Expression<Func<TestEntity, string>> assignor = s => s.Name;
        Expression<Func<TestResult, string>> assignee = d => d.Name;
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        var predicate = builder.Add(assignor, assignee).Build();
        // Assert
        Assert.Equal(1, predicate?.Parameters.Count);
        Assert.Equal(typeof(TestEntity), predicate?.Parameters[0].Type);
        Assert.NotNull(predicate);
    }

    #endregion

    #region Integration/Functional

    [Fact]
    public void Add_MultipleColumns_And_Build_Works()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        builder.Add("Name", "Name");
        builder.Add("Age", "Age");
        // Act
        var expr = builder.Build();
        var compiled = expr!.Compile();
        var result = compiled(new TestEntity { Name = "A", Age = 42 });
        // Assert
        Assert.NotNull(expr);
        Assert.Equal("A", result.Name);
        Assert.Equal(42, result.Age);
    }

    [Fact]
    public void QuerySelectBuilderT_IsAssignableToQuerySelectBuilderTT()
    {
        // Arrange & Act
        var builder = new QuerySelectBuilder<TestEntity>();
        // Assert
        Assert.IsType<QuerySelectBuilder<TestEntity, TestEntity>>(builder, exactMatch: false);
        builder.Add("Name");
        Assert.Single(builder.SelectDescriptorList);
    }

    [Fact]
    public void GetQuery_ShouldReturnQueryableWithSelectedProperties()
    {
        // Arrange
        Expression<Func<TestEntity, int>> idAssignor = s => s.Id;
        Expression<Func<TestResult, int>> idAssignee = d => d.Id;
        Expression<Func<TestEntity, string>> nameAssignor = s => s.Name;
        Expression<Func<TestResult, string>> nameAssignee = d => d.Name;
        var data = new[]
        {
            new TestEntity { Id = 1, Name = "John", Age = 30 },
            new TestEntity { Id = 2, Name = "Alice", Age = 25 },
            new TestEntity { Id = 3, Name = "Bob", Age = 35 }
        }.AsQueryable();
        var builder = new QuerySelectBuilder<TestEntity, TestResult>()
            .Add(idAssignor, idAssignee)
            .Add(nameAssignor, nameAssignee);
        // Act
        var result = data.Select(builder?.Build()!);
        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public void GetQuery_WithSameTAndTResult_ShouldReturnQueryableWithSelectedProperties()
    {
        // Arrange
        Expression<Func<TestEntity, int>> idAssignor = s => s.Id;
        Expression<Func<TestEntity, string>> nameAssignor = s => s.Name;
        var data = new[]
        {
            new TestEntity { Id = 1, Name = "John", Age = 30 },
            new TestEntity { Id = 2, Name = "Alice", Age = 25 },
            new TestEntity { Id = 3, Name = "Bob", Age = 35 }
        }.AsQueryable();
        var builder = new QuerySelectBuilder<TestEntity>()
            .Add(idAssignor)
            .Add(nameAssignor);
        // Act
        var result = data.Select(builder.Build()!);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public void GetQuery_WithNonMatchingTAndTResult_ShouldReturnQueryableWithMappedProperties()
    {
        // Arrange
        Expression<Func<TestEntity, int>> idAssignor = s => s.Id;
        Expression<Func<TestEntity, string>> nameAssignor = s => s.Name;
        Expression<Func<TestEntity, int>> ageAssignor = s => s.Age;
        var data = new[]
        {
            new TestEntity { Id = 1, Name = "John", Age = 30 },
            new TestEntity { Id = 2, Name = "Alice", Age = 25 },
            new TestEntity { Id = 3, Name = "Bob", Age = 35 }
        }.AsQueryable();
        var builder = new QuerySelectBuilder<TestEntity, TestResult>()
            .Add(idAssignor)
            .Add(nameAssignor)
            .Add(ageAssignor);
        // Act
        var result = data.Select(builder.Build()!);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    #endregion

    #region Edge/Negative Cases for Add (TResult)

    [Fact]
    public void AddColumn_Should_Throw_Exception_When_TResult_But_NoAssignTo_If_Not_Same_Property()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        Expression<Func<TestEntity, bool>> assignor = s => s.IsActive;
        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add(assignor));
    }

    [Fact]
    public void AddColumn_Should_Throw_Exception_When_TResult_But_AssignToIsNull()
    {
        // Arrange
        Expression<Func<TestEntity, string>> assignor = s => s.Name;
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(assignor, null!));
    }

    [Fact]
    public void AddColumn_Should_Throw_Exception_When_TResultSameAsT_But_AssignToIsPassed()
    {
        // Arrange
        Expression<Func<TestEntity, string>> assignor = s => s.Name;
        Expression<Func<TestResult, string>> assignee = d => d.Name;
        var builder = new QuerySelectBuilder<TestEntity, TestEntity>();
        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add(assignor, assignee));
    }

    #endregion

    #region Coverage for Expression/MemberInit

    [Fact]
    public void AddColumn_Should_Add_Column_To_SelectList()
    {
        // Arrange
        Expression<Func<TestEntity, string>> assignor = s => s.Name;
        Expression<Func<TestResult, string>> assignee = d => d.Name;
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add(assignor, assignee);
        // Assert
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
        Assert.Equal(typeof(TestEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
    }

    [Fact]
    public void AddExpressionColumn_Should_Add_Column_To_SelectList()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add(s => s.Name, d => d.Name);
        // Assert
        Assert.Equal(typeof(TestEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
    }

    [Fact]
    public void AddPropertyName_Should_Add_Column_To_SelectList()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add("Name", "Name");
        // Assert
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
        Assert.Equal(typeof(TestEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
    }

    [Fact]
    public void AddExpressionColumn_Should_Add_Column_When_TResult_But_NoAssignTo_If_Same_Property()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add(s => s.Name);
        // Assert
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
        Assert.Equal(typeof(TestEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
    }

    [Fact]
    public void AddPropertyName_Should_Add_Column_When_TResult_But_NoAssignTo_If_Same_Property()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add("Name");
        // Assert
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
        Assert.Equal(typeof(TestEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
    }

    [Fact]
    public void BuildAnonymousSelect_ShouldConstructExpressionForObjectType()
    {
        // Arrange
        var data = new[]
        {
            new TestEntity { Id = 1, Name = "John", Age = 30 },
            new TestEntity { Id = 2, Name = "Alice", Age = 25 },
            new TestEntity { Id = 3, Name = "Bob", Age = 35 }
        }.AsQueryable();
        var selectBuilder = new QuerySelectBuilder<TestEntity, object>()
            .Add(x => x.Name);
        // Act
        var selectExpression = selectBuilder.Build();
        var result = data.Select(selectExpression!);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Single(selectExpression?.Parameters!);
    }

    [Fact]
    public void Add_DuplicateColumns_AddsBoth()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        // Act
        builder.Add(s => s.Name, d => d.Name);
        builder.Add(s => s.Name, d => d.Name);
        // Assert
        Assert.Equal(2, builder.SelectDescriptorList.Count);
    }

    #endregion
}
