using System.Linq.Expressions;
using System.Text.Json;

namespace Craft.QuerySpec.Tests.Builders;

public class QuerySelectBuilderTests
{
    private readonly JsonSerializerOptions serializeOptions;

    public QuerySelectBuilderTests()
    {
        serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new QuerySelectBuilderJsonConverter<MyEntity, MyResult>());
    }

    [Fact]
    public void AddColumn_Should_Add_Column_To_SelectList()
    {
        // Arrange
        Expression<Func<MyEntity, string>> assignor = s => s.Name;
        Expression<Func<MyResult, string>> assignee = d => d.Name;

        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        builder.Add(assignor, assignee);

        // Assert
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
        Assert.Equal(typeof(MyEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
    }

    [Fact]
    public void AddExpressionColumn_Should_Add_Column_To_SelectList()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        builder.Add(s => s.Name, d => d.Name);

        // Assert
        Assert.Equal(typeof(MyEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
    }

    [Fact]
    public void AddPropertyName_Should_Add_Column_To_SelectList()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        builder.Add("Name", "Name");

        // Assert
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
        Assert.Equal(typeof(MyEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
    }

    [Fact]
    public void AddExpressionColumn_Should_Add_Column_When_TResult_But_NoAssignTo_If_Same_Property()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        builder.Add(s => s.Name);

        // Assert
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
        Assert.Equal(typeof(MyEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
    }

    [Fact]
    public void BuildAnonymousSelect_ShouldConstructExpressionForObjectType()
    {
        // Arrange
        var data = new[]
        {
            new MyEntity { Id = 1, Name = "John", Age = 30 },
            new MyEntity { Id = 2, Name = "Alice", Age = 25 },
            new MyEntity { Id = 3, Name = "Bob", Age = 35 }
        }.AsQueryable();
        var selectBuilder = new QuerySelectBuilder<MyEntity, object>()
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
    public void AddPropertyName_Should_Add_Column_When_TResult_But_NoAssignTo_If_Same_Property()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        builder.Add("Name");

        // Assert
        Assert.NotNull(builder.Build());
        Assert.Single(builder?.Build()?.Parameters!);
        Assert.Equal(typeof(MyEntity), builder?.Build()?.Parameters[0].Type);
        Assert.IsType<MemberInitExpression>(builder?.Build()?.Body);
    }

    [Fact]
    public void AddColumn_Should_Throw_Exception_When_TResult_But_NoAssignTo_If_Not_Same_Property()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();
        Expression<Func<MyEntity, bool>> assignor = s => s.IsActive;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add(assignor));
    }

    [Fact]
    public void AddColumn_Should_Throw_Exception_When_TResult_But_AssignToIsNull()
    {
        // Arrange
        Expression<Func<MyEntity, string>> assignor = s => s.Name;

        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(assignor, null!));
    }

    [Fact]
    public void AddColumn_Should_Throw_Exception_When_TResultSameAsT_But_AssignToIsPassed()
    {
        // Arrange
        Expression<Func<MyEntity, string>> assignor = s => s.Name;
        Expression<Func<MyResult, string>> assignee = d => d.Name;

        var builder = new QuerySelectBuilder<MyEntity, MyEntity>();

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => builder.Add(assignor, assignee));
    }

    [Fact]
    public void Build_Should_Create_SelectPredicate_When_TResult_Equals_T()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyEntity>();

        // Act & Assert
        // Now, Build() should throw an InvalidOperationException if no select mappings are defined
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_Should_Create_SelectPredicateForResult_When_TResult_Not_T()
    {
        // Arrange
        Expression<Func<MyEntity, string>> assignor = s => s.Name;
        Expression<Func<MyResult, string>> assignee = d => d.Name;

        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        var predicate = builder.Add(assignor, assignee).Build();

        // Assert
        Assert.Equal(1, predicate?.Parameters.Count);
        Assert.Equal(typeof(MyEntity), predicate?.Parameters[0].Type);
        Assert.NotNull(predicate);
    }

    [Fact]
    public void GetQuery_ShouldReturnQueryableWithSelectedProperties()
    {
        // Arrange
        Expression<Func<MyEntity, int>> idAssignor = s => s.Id;
        Expression<Func<MyResult, int>> idAssignee = d => d.Id;
        Expression<Func<MyEntity, string>> nameAssignor = s => s.Name;
        Expression<Func<MyResult, string>> nameAssignee = d => d.Name;

        var data = new[]
        {
            new MyEntity { Id = 1, Name = "John", Age = 30 },
            new MyEntity { Id = 2, Name = "Alice", Age = 25 },
            new MyEntity { Id = 3, Name = "Bob", Age = 35 }
        }.AsQueryable();

        var builder = new QuerySelectBuilder<MyEntity, MyResult>()
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
        Expression<Func<MyEntity, int>> idAssignor = s => s.Id;
        Expression<Func<MyEntity, string>> nameAssignor = s => s.Name;
        var data = new[]
        {
            new MyEntity { Id = 1, Name = "John", Age = 30 },
            new MyEntity { Id = 2, Name = "Alice", Age = 25 },
            new MyEntity { Id = 3, Name = "Bob", Age = 35 }
        }.AsQueryable();

        var builder = new QuerySelectBuilder<MyEntity>()
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
        Expression<Func<MyEntity, int>> idAssignor = s => s.Id;
        Expression<Func<MyEntity, string>> nameAssignor = s => s.Name;
        Expression<Func<MyEntity, int>> ageAssignor = s => s.Age;

        var data = new[]
        {
            new MyEntity { Id = 1, Name = "John", Age = 30 },
            new MyEntity { Id = 2, Name = "Alice", Age = 25 },
            new MyEntity { Id = 3, Name = "Bob", Age = 35 }
        }.AsQueryable();

        var builder = new QuerySelectBuilder<MyEntity, MyResult>()
            .Add(idAssignor)
            .Add(nameAssignor)
            .Add(ageAssignor);

        // Act
        var result = data.Select(builder.Build()!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public void Clear_ShouldRemoveAllSelectExpressions()
    {
        // Arrange
        Expression<Func<MyEntity, string>> assignor = s => s.Name;
        Expression<Func<MyResult, string>> assignee = d => d.Name;

        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        builder.Clear();

        // Assert
        Assert.Equal(0, builder.Count);
    }

    [Fact]
    public void CanConvert_ReturnsTrueForCorrectType()
    {
        // Arrange
        var converter = new QuerySelectBuilderJsonConverter<MyEntity, MyResult>();

        // Act
        bool canConvert = converter.CanConvert(typeof(QuerySelectBuilder<MyEntity, MyResult>));

        // Assert
        Assert.True(canConvert);
    }

    [Fact]
    public void CanConvert_ReturnsFalseForIncorrectType()
    {
        // Arrange
        var converter = new QuerySelectBuilderJsonConverter<MyEntity, MyResult>();

        // Act
        bool canConvert = converter.CanConvert(typeof(string));

        // Assert
        Assert.False(canConvert);
    }

    [Fact]
    public void Read_ThrowsJsonExceptionForInvalidFormat()
    {
        // Arrange
        const string invalidJson = "{}"; // Not an array

        // Act and Assert
        void act() => JsonSerializer.Deserialize<QuerySelectBuilder<MyEntity, MyResult>>(invalidJson, serializeOptions);
        Assert.Throws<JsonException>(act);
    }

    [Fact]
    public void Write_SerializesEntityFilterBuilderToJsonCorrectly()
    {
        // Arrange
        var querySelectBuilder = new QuerySelectBuilder<MyEntity, MyResult>();
        querySelectBuilder.Add(new SelectDescriptor<MyEntity, MyResult>("Name", "Name"));

        // Act
        var json = JsonSerializer.Serialize(querySelectBuilder, serializeOptions);

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Equal("[{\"Assignor\":\"Name\",\"Assignee\":\"Name\"}]", json); 
    }

    [Fact]
    public void Read_DeserializesValidJsonToEntityFilterBuilder()
    {
        // Arrange
        const string validJson = "[{\"Assignor\":\"Name\",\"Assignee\":\"Name\"}]";

        // Act
        var querySelectBuilder = JsonSerializer
            .Deserialize<QuerySelectBuilder<MyEntity, MyResult>>(validJson, serializeOptions);

        // Assert
        Assert.NotNull(querySelectBuilder);
        Assert.Equal(1, querySelectBuilder.Count);
        Assert.Single(querySelectBuilder.SelectDescriptorList);
        Assert.Contains("x.Name", querySelectBuilder.SelectDescriptorList[0].Assignee?.Body.ToString());
        Assert.Contains("x.Name", querySelectBuilder.SelectDescriptorList[0].Assignor?.Body.ToString());
    }

    [Fact]
    public void Add_NullAssignor_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<MyEntity, object>>)null!, d => d.Name));
    }

    [Fact]
    public void Add_NullAssignee_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();
        Expression<Func<MyEntity, object>> assignor = s => s.Name;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add(assignor, (Expression<Func<MyResult, object>>)null!));
    }

    [Fact]
    public void Add_NullSelectDescriptor_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((SelectDescriptor<MyEntity, MyResult>)null!));
    }

    [Fact]
    public void Add_NullLambdaColumn_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((LambdaExpression)null!));
    }

    [Fact]
    public void Add_InvalidPropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add("NotAProp", "Name"));
        Assert.Throws<ArgumentException>(() => builder.Add("Name", "NotAProp"));
    }

    [Fact]
    public void Clear_AfterAddingColumns_EmptiesList()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        builder.Add(s => s.Name, d => d.Name);
        builder.Clear();

        // Assert
        Assert.Empty(builder.SelectDescriptorList);
    }

    [Fact]
    public void Write_SerializesEmptyQuerySelectBuilderToJsonCorrectly()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        var json = JsonSerializer.Serialize(builder, serializeOptions);

        // Assert
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Read_DeserializesEmptyArrayToQuerySelectBuilder()
    {
        // Arrange
        const string emptyJson = "[]";

        // Act
        var builder = JsonSerializer
            .Deserialize<QuerySelectBuilder<MyEntity, MyResult>>(emptyJson, serializeOptions);

        // Assert
        Assert.NotNull(builder);
        Assert.Empty(builder.SelectDescriptorList);
    }

    [Fact]
    public void Add_DuplicateColumns_AddsBoth()
    {
        // Arrange
        var builder = new QuerySelectBuilder<MyEntity, MyResult>();

        // Act
        builder.Add(s => s.Name, d => d.Name);
        builder.Add(s => s.Name, d => d.Name);

        // Assert
        Assert.Equal(2, builder.SelectDescriptorList.Count);
    }

    private class MyEntity
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private class MyResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}

