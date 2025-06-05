using System.Linq.Expressions;
using Craft.Extensions.Expressions;

namespace Craft.Extensions.Tests.Expressions;

public class ConditionRemoverTests
{
    [Fact]
    public void RemoveCondition_WithDifferentConditions_ShouldReturnOriginalExpression()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5;
        Expression<Func<int, bool>> conditionToRemove = x => x < 10;

        // Act
        var result = original.RemoveCondition(conditionToRemove);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(original.ToString(), result.ToString());
    }

    [Fact]
    public void RemoveCondition_WithEquivalentConditions_ShouldReturnNull()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5;
        Expression<Func<int, bool>> equivalentCondition = x => x > 5;

        // Act
        var result = original.RemoveCondition(equivalentCondition);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RemoveCondition_WithNullConditionToRemove_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5;
        Expression<Func<int, bool>> conditionToRemove = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => original.RemoveCondition(conditionToRemove));
    }

    [Fact]
    public void RemoveCondition_WithNullOriginalExpression_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<int, bool>> original = null!;
        Expression<Func<int, bool>> conditionToRemove = x => x < 10;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => original.RemoveCondition(conditionToRemove));
    }

    [Fact]
    public void RemoveCondition_WithRemoveCondition_ShouldReturnReducedExpression()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5 && x < 10;
        Expression<Func<int, bool>> conditionToRemove = x => x < 10;
        Expression<Func<int, bool>> expected = x => x > 5;

        // Act
        var result = original.RemoveCondition(conditionToRemove);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void RemoveConditions_WithNullExpression_Throws()
    {
        // Arrange
        Expression<Func<int, bool>> original = null!;
        Expression<Func<int, bool>> cond1 = x => x > 5;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => original.RemoveConditions(cond1));
    }

    [Fact]
    public void RemoveConditions_WithNullOrEmptyConditions_ReturnsOriginal()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5;

        // Act
        var result1 = original.RemoveConditions();
        var result2 = original.RemoveConditions(null!);

        // Assert
        Assert.Equal(original.ToString(), result1?.ToString());
        Assert.Equal(original.ToString(), result2?.ToString());
    }

    [Fact]
    public void RemoveConditions_RemovesMultipleConditions()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5 && x < 10 && x != 7;
        Expression<Func<int, bool>> cond1 = x => x < 10;
        Expression<Func<int, bool>> cond2 = x => x != 7;
        Expression<Func<int, bool>> expected = x => x > 5;

        // Act
        var result = original.RemoveConditions(cond1, cond2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void RemoveConditions_ReturnsNullIfAllRemoved()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5;
        Expression<Func<int, bool>> cond1 = x => x > 5;

        // Act
        var result = original.RemoveConditions(cond1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReplaceCondition_ReplacesCondition()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5 && x < 10;
        Expression<Func<int, bool>> oldCond = x => x < 10;
        Expression<Func<int, bool>> newCond = x => x < 20;
        Expression<Func<int, bool>> expected = x => x > 5 && x < 20;

        // Act
        var result = original.ReplaceCondition(oldCond, newCond);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ReplaceCondition_ThrowsOnNullArguments()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5 && x < 10;
        Expression<Func<int, bool>> oldCond = x => x < 10;
        Expression<Func<int, bool>> newCond = x => x < 20;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ConditionRemover.ReplaceCondition<int>(null!, oldCond, newCond));
        Assert.Throws<ArgumentNullException>(() => original.ReplaceCondition(null!, newCond));
        Assert.Throws<ArgumentNullException>(() => original.ReplaceCondition(oldCond, null!));
    }

    [Fact]
    public void ReplaceCondition_DoesNothingIfOldConditionNotFound()
    {
        // Arrange
        Expression<Func<int, bool>> original = x => x > 5 && x < 10;
        Expression<Func<int, bool>> oldCond = x => x != 0;
        Expression<Func<int, bool>> newCond = x => x < 20;

        // Act
        var result = original.ReplaceCondition(oldCond, newCond);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(original.ToString(), result.ToString());
    }

    [Fact]
    public void SameExpression_ShouldBeEquivalent()
    {
        // Arrange
        Expression<Func<Entity, bool>> expr = e => e.IsActive;

        // Act & Assert
        Assert.True(ConditionRemover.IsEquivalentCondition(expr, expr));
    }

    [Fact]
    public void EquivalentBooleanCheck_ShouldBeEquivalent()
    {
        // Arrange
        Expression<Func<Entity, bool>> expr1 = e => e.IsActive;
        Expression<Func<Entity, bool>> expr2 = e => e.IsActive == true;

        // Act & Assert
        Assert.True(ConditionRemover.IsEquivalentCondition(expr1, expr2));
    }

    [Fact]
    public void NegatedBooleanCheck_ShouldBeEquivalent()
    {
        // Arrange
        Expression<Func<Entity, bool>> expr1 = e => !e.IsActive;
        Expression<Func<Entity, bool>> expr2 = e => e.IsActive == false;

        // Act & Assert
        Assert.True(ConditionRemover.IsEquivalentCondition(expr1, expr2));
    }

    [Fact]
    public void DifferentExpressions_ShouldNotBeEquivalent()
    {
        // Arrange
        Expression<Func<Entity, bool>> expr1 = e => e.IsActive;
        Expression<Func<Entity, bool>> expr2 = e => e.IsDeleted;

        // Act & Assert
        Assert.False(ConditionRemover.IsEquivalentCondition(expr1, expr2));
    }

    [Fact]
    public void CommutativeEquality_ShouldBeEquivalent()
    {
        // Arrange
        Expression<Func<Entity, bool>> expr1 = e => e.Id == 5;
        Expression<Func<Entity, bool>> expr2 = e => 5 == e.Id;

        // Act & Assert
        Assert.True(ConditionRemover.IsEquivalentCondition(expr1, expr2));
    }

    [Fact]
    public void NonCommutativeInequality_ShouldNotBeEquivalent()
    {
        // Arrange
        Expression<Func<Entity, bool>> expr1 = e => e.Id > 5;
        Expression<Func<Entity, bool>> expr2 = e => 5 < e.Id;

        // Act & Assert
        Assert.False(ConditionRemover.IsEquivalentCondition(expr1, expr2));
    }

    [Fact]
    public void StringComparison_ShouldNotBeEquivalent()
    {
        // Arrange
        Expression<Func<Entity, bool>> expr1 = e => e.Name == "Alice";
        Expression<Func<Entity, bool>> expr2 = e => e.Name == "Bob";

        // Act & Assert
        Assert.False(ConditionRemover.IsEquivalentCondition(expr1, expr2));
    }

    [Fact]
    public void IsEquivalentCondition_Should_Recognize_Semantic_Equality()
    {
        // Arrange
        Expression<Func<Entity, bool>> expr1 = e => e.IsActive;
        Expression<Func<Entity, bool>> expr2 = e => e.IsActive == true;
        Expression<Func<Entity, bool>> expr3 = e => true == e.IsActive;
        Expression<Func<Entity, bool>> expr4 = e => e.IsActive != false;

        // Act & Assert
        Assert.True(ConditionRemover.IsEquivalentCondition(expr1, expr2));
        Assert.True(ConditionRemover.IsEquivalentCondition(expr1, expr3));
        Assert.True(ConditionRemover.IsEquivalentCondition(expr1, expr4));
    }

    private class Entity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? Name { get; set; }
    }
}
