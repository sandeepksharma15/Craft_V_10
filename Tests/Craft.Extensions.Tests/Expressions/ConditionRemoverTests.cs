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
        Expression<Func<int, bool>> original = null!;
        Expression<Func<int, bool>> cond1 = x => x > 5;
        Assert.Throws<ArgumentNullException>(() => original.RemoveConditions(cond1));
    }

    [Fact]
    public void RemoveConditions_WithNullOrEmptyConditions_ReturnsOriginal()
    {
        Expression<Func<int, bool>> original = x => x > 5;
        var result1 = original.RemoveConditions();
        var result2 = original.RemoveConditions(null!);
        Assert.Equal(original.ToString(), result1?.ToString());
        Assert.Equal(original.ToString(), result2?.ToString());
    }

    [Fact]
    public void RemoveConditions_RemovesMultipleConditions()
    {
        Expression<Func<int, bool>> original = x => x > 5 && x < 10 && x != 7;
        Expression<Func<int, bool>> cond1 = x => x < 10;
        Expression<Func<int, bool>> cond2 = x => x != 7;
        Expression<Func<int, bool>> expected = x => x > 5;

        var result = original.RemoveConditions(cond1, cond2);

        Assert.NotNull(result);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void RemoveConditions_ReturnsNullIfAllRemoved()
    {
        Expression<Func<int, bool>> original = x => x > 5;
        Expression<Func<int, bool>> cond1 = x => x > 5;
        var result = original.RemoveConditions(cond1);
        Assert.Null(result);
    }

    [Fact]
    public void ReplaceCondition_ReplacesCondition()
    {
        Expression<Func<int, bool>> original = x => x > 5 && x < 10;
        Expression<Func<int, bool>> oldCond = x => x < 10;
        Expression<Func<int, bool>> newCond = x => x < 20;
        Expression<Func<int, bool>> expected = x => x > 5 && x < 20;

        var result = original.ReplaceCondition(oldCond, newCond);

        Assert.NotNull(result);
        Assert.Equal(expected.ToString(), result.ToString());
    }

    [Fact]
    public void ReplaceCondition_ThrowsOnNullArguments()
    {
        Expression<Func<int, bool>> original = x => x > 5 && x < 10;
        Expression<Func<int, bool>> oldCond = x => x < 10;
        Expression<Func<int, bool>> newCond = x => x < 20;

        Assert.Throws<ArgumentNullException>(() => ConditionRemover.ReplaceCondition<int>(null!, oldCond, newCond));
        Assert.Throws<ArgumentNullException>(() => original.ReplaceCondition(null!, newCond));
        Assert.Throws<ArgumentNullException>(() => original.ReplaceCondition(oldCond, null!));
    }

    [Fact]
    public void ReplaceCondition_DoesNothingIfOldConditionNotFound()
    {
        Expression<Func<int, bool>> original = x => x > 5 && x < 10;
        Expression<Func<int, bool>> oldCond = x => x != 0;
        Expression<Func<int, bool>> newCond = x => x < 20;

        var result = original.ReplaceCondition(oldCond, newCond);

        Assert.NotNull(result);
        Assert.Equal(original.ToString(), result.ToString());
    }
}
