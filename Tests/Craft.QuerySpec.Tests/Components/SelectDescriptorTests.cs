using System.Linq.Expressions;

namespace Craft.QuerySpec.Tests.Components;

public class SelectDescriptorTests
{
    private class Source
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class Dest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class DestNoName
    {
        public int Age { get; set; }
    }

    [Fact]
    public void Ctor_AssignorLambda_TResultObject_SetsAssignorOnly()
    {
        Expression<Func<Source, object>> assignor = x => x.Name;
        var desc = new SelectDescriptor<Source, object>(assignor);
        Assert.Equal(assignor, desc.Assignor);
        Assert.Null(desc.Assignee);
    }

    [Fact]
    public void Ctor_AssignorLambda_TResultHasProperty_SetsAssignee()
    {
        Expression<Func<Source, object>> assignor = x => x.Name;
        var desc = new SelectDescriptor<Source, Dest>(assignor);
        Assert.Equal(assignor, desc.Assignor);
        Assert.NotNull(desc.Assignee);
        Assert.Equal("Name", ((MemberExpression)desc.Assignee!.Body).Member.Name);
    }

    [Fact]
    public void Ctor_AssignorLambda_TResultMissingProperty_Throws()
    {
        Expression<Func<Source, object>> assignor = x => x.Name;
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, DestNoName>(assignor));
    }

    [Fact]
    public void Ctor_AssignorLambda_Null_Throws()
    {
        // Disambiguate by casting to LambdaExpression
        Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Source, object>((LambdaExpression)null!));
    }

    [Fact]
    public void Ctor_AssignorLambda_NotMemberExpression_Throws()
    {
        Expression<Func<Source, object>> assignor = x => x.Name + "!";
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, object>(assignor));
    }

    [Fact]
    public void Ctor_AssignorAndAssigneeLambda_Valid_SetsBoth()
    {
        Expression<Func<Source, object>> assignor = x => x.Name;
        Expression<Func<Dest, object>> assignee = x => x.Name;
        var desc = new SelectDescriptor<Source, Dest>(assignor, assignee);
        Assert.Equal(assignor, desc.Assignor);
        Assert.Equal(assignee, desc.Assignee);
    }

    [Fact]
    public void Ctor_AssignorAndAssigneeLambda_AssignorNull_Throws()
    {
        Expression<Func<Dest, object>> assignee = x => x.Name;
        Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Source, Dest>(null!, assignee));
    }

    [Fact]
    public void Ctor_AssignorAndAssigneeLambda_AssigneeNull_Throws()
    {
        Expression<Func<Source, object>> assignor = x => x.Name;
        Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Source, Dest>(assignor, null!));
    }

    [Fact]
    public void Ctor_AssignorAndAssigneeLambda_AssignorNotMember_Throws()
    {
        Expression<Func<Source, object>> assignor = x => x.Name + "!";
        Expression<Func<Dest, object>> assignee = x => x.Name;
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, Dest>(assignor, assignee));
    }

    [Fact]
    public void Ctor_AssignorAndAssigneeLambda_AssigneeNotMember_Throws()
    {
        Expression<Func<Source, object>> assignor = x => x.Name;
        Expression<Func<Dest, object>> assignee = x => x.Name + "!";
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, Dest>(assignor, assignee));
    }

    [Fact]
    public void Ctor_AssignorAndAssigneeLambda_TResultIsT_Throws()
    {
        Expression<Func<Source, object>> assignor = x => x.Name;
        Expression<Func<Source, object>> assignee = x => x.Name;
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, Source>(assignor, assignee));
    }

    [Fact]
    public void Ctor_AssignorAndAssigneeLambda_TResultIsObject_Throws()
    {
        Expression<Func<Source, object>> assignor = x => x.Name;
        Expression<Func<object, object>> assignee = x => x;
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, object>(assignor, assignee));
    }

    [Fact]
    public void Ctor_AssignorPropName_TResultObject_SetsAssignorOnly()
    {
        var desc = new SelectDescriptor<Source, object>("Name");
        Assert.NotNull(desc.Assignor);
        Assert.Null(desc.Assignee);
    }

    [Fact]
    public void Ctor_AssignorPropName_TResultHasProperty_SetsAssignee()
    {
        var desc = new SelectDescriptor<Source, Dest>("Name");
        Assert.NotNull(desc.Assignor);
        Assert.NotNull(desc.Assignee);
        Assert.Equal("Name", ((MemberExpression)desc.Assignee!.Body).Member.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Ctor_AssignorPropName_NullOrWhitespace_Throws(string? propName)
    {
        if (propName is null)
            Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Source, object>(propName!));
        else
            Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, object>(propName!));
    }

    [Fact]
    public void Ctor_AssignorPropName_TResultMissingProperty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, DestNoName>("Name"));
    }

    [Fact]
    public void Ctor_AssignorAndAssigneePropNames_Valid_SetsBoth()
    {
        var desc = new SelectDescriptor<Source, Dest>("Name", "Name");
        Assert.NotNull(desc.Assignor);
        Assert.NotNull(desc.Assignee);
        Assert.Equal("Name", ((MemberExpression)desc.Assignee!.Body).Member.Name);
    }

    [Theory]
    [InlineData(null, "Name")]
    [InlineData("", "Name")]
    [InlineData(" ", "Name")]
    [InlineData("Name", null)]
    [InlineData("Name", "")]
    [InlineData("Name", " ")]
    public void Ctor_AssignorAndAssigneePropNames_NullOrWhitespace_Throws(string? assignor, string? assignee)
    {
        if (assignor is null || assignee is null)
            Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Source, Dest>(assignor!, assignee!));
        else
            Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, Dest>(assignor!, assignee!));
    }

    [Fact]
    public void Ctor_AssignorAndAssigneePropNames_AssignorNotOnT_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, Dest>("NotAProp", "Name"));
    }

    [Fact]
    public void Ctor_AssignorAndAssigneePropNames_AssigneeNotOnTResult_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Source, Dest>("Name", "NotAProp"));
    }

    [Fact]
    public void InternalParameterlessCtor_CanInstantiate()
    {
        var ctor = typeof(SelectDescriptor<Source, Dest>).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, Type.EmptyTypes, null);
        var instance = ctor!.Invoke(null);
        Assert.NotNull(instance);
    }
}
