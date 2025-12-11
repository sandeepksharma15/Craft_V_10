using System.Text.Json;

namespace Craft.Auditing.Tests.Extensions;

public class AuditTrailExtensionsTests
{
    [Fact]
    public void GetOldValuesAsDictionary_WithValidJson_ReturnsDictionary()
    {
        // Arrange
        var audit = new AuditTrail
        {
            OldValues = "{\"Name\":\"OldName\",\"Price\":100}"
        };

        // Act
        var result = audit.GetOldValuesAsDictionary();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("Name"));
        Assert.True(result.ContainsKey("Price"));
    }

    [Fact]
    public void GetOldValuesAsDictionary_WithNull_ReturnsNull()
    {
        // Arrange
        var audit = new AuditTrail { OldValues = null };

        // Act
        var result = audit.GetOldValuesAsDictionary();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetOldValuesAsDictionary_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        var audit = new AuditTrail { OldValues = "invalid json" };

        // Act
        var result = audit.GetOldValuesAsDictionary();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNewValuesAsDictionary_WithValidJson_ReturnsDictionary()
    {
        // Arrange
        var audit = new AuditTrail
        {
            NewValues = "{\"Name\":\"NewName\",\"Price\":200}"
        };

        // Act
        var result = audit.GetNewValuesAsDictionary();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("Name"));
        Assert.True(result.ContainsKey("Price"));
    }

    [Fact]
    public void GetKeyValuesAsDictionary_WithValidJson_ReturnsDictionary()
    {
        // Arrange
        var audit = new AuditTrail
        {
            KeyValues = "{\"Id\":123}"
        };

        // Act
        var result = audit.GetKeyValuesAsDictionary();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.ContainsKey("Id"));
    }

    [Fact]
    public void GetChangedColumnsAsList_WithValidJson_ReturnsList()
    {
        // Arrange
        var audit = new AuditTrail
        {
            ChangedColumns = "[\"Name\",\"Price\",\"Description\"]"
        };

        // Act
        var result = audit.GetChangedColumnsAsList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains("Name", result);
        Assert.Contains("Price", result);
        Assert.Contains("Description", result);
    }

    [Fact]
    public void GetChangedColumnsAsList_WithNull_ReturnsNull()
    {
        // Arrange
        var audit = new AuditTrail { ChangedColumns = null };

        // Act
        var result = audit.GetChangedColumnsAsList();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeserializeOldValues_WithValidType_ReturnsObject()
    {
        // Arrange
        var testData = new TestProduct { Name = "Product", Price = 100 };
        var audit = new AuditTrail
        {
            OldValues = JsonSerializer.Serialize(testData)
        };

        // Act
        var result = audit.DeserializeOldValues<TestProduct>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Product", result.Name);
        Assert.Equal(100, result.Price);
    }

    [Fact]
    public void DeserializeOldValues_WithNull_ReturnsNull()
    {
        // Arrange
        var audit = new AuditTrail { OldValues = null };

        // Act
        var result = audit.DeserializeOldValues<TestProduct>();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeserializeNewValues_WithValidType_ReturnsObject()
    {
        // Arrange
        var testData = new TestProduct { Name = "Product", Price = 200 };
        var audit = new AuditTrail
        {
            NewValues = JsonSerializer.Serialize(testData)
        };

        // Act
        var result = audit.DeserializeNewValues<TestProduct>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Product", result.Name);
        Assert.Equal(200, result.Price);
    }

    [Fact]
    public void GetOldValue_WithExistingProperty_ReturnsValue()
    {
        // Arrange
        var audit = new AuditTrail
        {
            OldValues = "{\"Name\":\"OldName\"}"
        };

        // Act
        var result = audit.GetOldValue("Name");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetOldValue_WithNonExistingProperty_ReturnsNull()
    {
        // Arrange
        var audit = new AuditTrail
        {
            OldValues = "{\"Name\":\"OldName\"}"
        };

        // Act
        var result = audit.GetOldValue("NonExisting");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNewValue_WithExistingProperty_ReturnsValue()
    {
        // Arrange
        var audit = new AuditTrail
        {
            NewValues = "{\"Name\":\"NewName\"}"
        };

        // Act
        var result = audit.GetNewValue("Name");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void HasPropertyChanged_WithChangedProperty_ReturnsTrue()
    {
        // Arrange
        var audit = new AuditTrail
        {
            ChangedColumns = "[\"Name\",\"Price\"]"
        };

        // Act
        var result = audit.HasPropertyChanged("Name");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasPropertyChanged_WithUnchangedProperty_ReturnsFalse()
    {
        // Arrange
        var audit = new AuditTrail
        {
            ChangedColumns = "[\"Name\",\"Price\"]"
        };

        // Act
        var result = audit.HasPropertyChanged("Description");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetOldValue_Generic_WithValidType_ReturnsTypedValue()
    {
        // Arrange
        var audit = new AuditTrail
        {
            OldValues = "{\"Price\":100}"
        };

        // Act
        var result = audit.GetOldValue<int>("Price");

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void GetNewValue_Generic_WithValidType_ReturnsTypedValue()
    {
        // Arrange
        var audit = new AuditTrail
        {
            NewValues = "{\"Price\":200}"
        };

        // Act
        var result = audit.GetNewValue<int>("Price");

        // Assert
        Assert.Equal(200, result);
    }

    [Fact]
    public void GetOldValue_Generic_WithNonExistingProperty_ReturnsDefault()
    {
        // Arrange
        var audit = new AuditTrail
        {
            OldValues = "{\"Name\":\"Test\"}"
        };

        // Act
        var result = audit.GetOldValue<int>("Price");

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void GetNewValue_Generic_WithStringType_ReturnsString()
    {
        // Arrange
        var audit = new AuditTrail
        {
            NewValues = "{\"Name\":\"TestProduct\"}"
        };

        // Act
        var result = audit.GetNewValue<string>("Name");

        // Assert
        Assert.Equal("TestProduct", result);
    }

    [Fact]
    public void Extensions_WithNullAudit_ThrowsArgumentNullException()
    {
        // Arrange
        AuditTrail audit = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => audit.GetOldValuesAsDictionary());
        Assert.Throws<ArgumentNullException>(() => audit.GetNewValuesAsDictionary());
        Assert.Throws<ArgumentNullException>(() => audit.GetKeyValuesAsDictionary());
        Assert.Throws<ArgumentNullException>(() => audit.GetChangedColumnsAsList());
    }

    [Fact]
    public void GetValue_WithNullPropertyName_ThrowsArgumentNullException()
    {
        // Arrange
        var audit = new AuditTrail();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => audit.GetOldValue(null!));
        Assert.Throws<ArgumentNullException>(() => audit.GetNewValue(null!));
        Assert.Throws<ArgumentNullException>(() => audit.HasPropertyChanged(null!));
    }

    private class TestProduct
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
