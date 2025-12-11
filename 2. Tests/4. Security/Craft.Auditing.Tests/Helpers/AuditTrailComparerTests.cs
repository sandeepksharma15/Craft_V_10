namespace Craft.Auditing.Tests.Helpers;

public class AuditTrailComparerTests
{
    [Fact]
    public void CompareTo_WithNoPreviousAudit_ReturnsEmptyComparison()
    {
        // Arrange
        var current = new AuditTrail
        {
            TableName = "Products",
            NewValues = "{\"Name\":\"Product1\"}"
        };

        // Act
        var result = current.CompareTo(null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(current, result.CurrentAudit);
        Assert.Null(result.PreviousAudit);
        Assert.Empty(result.Differences);
        Assert.False(result.HasDifferences);
    }

    [Fact]
    public void CompareTo_WithDifferentTables_ThrowsArgumentException()
    {
        // Arrange
        var current = new AuditTrail { TableName = "Products" };
        var previous = new AuditTrail { TableName = "Orders" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => current.CompareTo(previous));
    }

    [Fact]
    public void CompareTo_WithChangedProperties_ReturnsDifferences()
    {
        // Arrange
        var current = new AuditTrail
        {
            TableName = "Products",
            NewValues = "{\"Name\":\"NewProduct\",\"Price\":200}"
        };
        var previous = new AuditTrail
        {
            TableName = "Products",
            NewValues = "{\"Name\":\"OldProduct\",\"Price\":100}"
        };

        // Act
        var result = current.CompareTo(previous);

        // Assert
        Assert.True(result.HasDifferences);
        Assert.Equal(2, result.Differences.Count);
        Assert.Contains(result.Differences, d => d.PropertyName == "Name" && d.HasChanged);
        Assert.Contains(result.Differences, d => d.PropertyName == "Price" && d.HasChanged);
    }

    [Fact]
    public void CompareTo_WithUnchangedProperties_ReturnsNoDifferences()
    {
        // Arrange
        var current = new AuditTrail
        {
            TableName = "Products",
            NewValues = "{\"Name\":\"Product\",\"Price\":100}"
        };
        var previous = new AuditTrail
        {
            TableName = "Products",
            NewValues = "{\"Name\":\"Product\",\"Price\":100}"
        };

        // Act
        var result = current.CompareTo(previous);

        // Assert
        Assert.True(result.HasDifferences);
        Assert.All(result.Differences, d => Assert.False(d.HasChanged));
    }

    [Fact]
    public void PropertyWasModified_WithModifiedProperty_ReturnsTrue()
    {
        // Arrange
        var audit = new AuditTrail
        {
            ChangeType = EntityChangeType.Updated,
            ChangedColumns = "[\"Name\",\"Price\"]"
        };

        // Act
        var result = audit.PropertyWasModified("Name");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PropertyWasModified_WithUnmodifiedProperty_ReturnsFalse()
    {
        // Arrange
        var audit = new AuditTrail
        {
            ChangeType = EntityChangeType.Updated,
            ChangedColumns = "[\"Name\",\"Price\"]"
        };

        // Act
        var result = audit.PropertyWasModified("Description");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PropertyWasModified_WithNonUpdatedChangeType_ReturnsFalse()
    {
        // Arrange
        var audit = new AuditTrail
        {
            ChangeType = EntityChangeType.Created,
            ChangedColumns = "[\"Name\"]"
        };

        // Act
        var result = audit.PropertyWasModified("Name");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetPropertyChanges_WithUpdatedEntity_ReturnsChanges()
    {
        // Arrange
        var audit = new AuditTrail
        {
            OldValues = "{\"Name\":\"OldName\",\"Price\":100}",
            NewValues = "{\"Name\":\"NewName\",\"Price\":200}"
        };

        // Act
        var result = audit.GetPropertyChanges();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.True(d.HasChanged));
    }

    [Fact]
    public void GetPropertyChanges_WithNoValues_ReturnsEmpty()
    {
        // Arrange
        var audit = new AuditTrail();

        // Act
        var result = audit.GetPropertyChanges();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetPropertyValue_WithUseNewValueTrue_ReturnsNewValue()
    {
        // Arrange
        var audit = new AuditTrail
        {
            OldValues = "{\"Name\":\"OldName\"}",
            NewValues = "{\"Name\":\"NewName\"}"
        };

        // Act
        var result = audit.GetPropertyValue("Name", useNewValue: true);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetPropertyValue_WithUseNewValueFalse_ReturnsOldValue()
    {
        // Arrange
        var audit = new AuditTrail
        {
            OldValues = "{\"Name\":\"OldName\"}",
            NewValues = "{\"Name\":\"NewName\"}"
        };

        // Act
        var result = audit.GetPropertyValue("Name", useNewValue: false);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetChangedProperties_WithUpdatedEntity_ReturnsChangedPairs()
    {
        // Arrange
        var audit = new AuditTrail
        {
            ChangeType = EntityChangeType.Updated,
            ChangedColumns = "[\"Name\",\"Price\"]",
            OldValues = "{\"Name\":\"OldName\",\"Price\":100}",
            NewValues = "{\"Name\":\"NewName\",\"Price\":200}"
        };

        // Act
        var result = audit.GetChangedProperties();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("Name", result.Keys);
        Assert.Contains("Price", result.Keys);
    }

    [Fact]
    public void GetChangedProperties_WithNonUpdatedEntity_ReturnsEmpty()
    {
        // Arrange
        var audit = new AuditTrail
        {
            ChangeType = EntityChangeType.Created
        };

        // Act
        var result = audit.GetChangedProperties();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetChangedProperties_WithNoChangedColumns_ReturnsEmpty()
    {
        // Arrange
        var audit = new AuditTrail
        {
            ChangeType = EntityChangeType.Updated,
            ChangedColumns = null
        };

        // Act
        var result = audit.GetChangedProperties();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void PropertyWasModified_WithNullPropertyName_ThrowsArgumentNullException()
    {
        // Arrange
        var audit = new AuditTrail();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => audit.PropertyWasModified(null!));
    }

    [Fact]
    public void GetPropertyValue_WithNullPropertyName_ThrowsArgumentNullException()
    {
        // Arrange
        var audit = new AuditTrail();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => audit.GetPropertyValue(null!));
    }

    [Fact]
    public void CompareTo_WithNullCurrent_ThrowsArgumentNullException()
    {
        // Arrange
        AuditTrail current = null!;
        var previous = new AuditTrail();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => current.CompareTo(previous));
    }
}
