namespace Craft.Auditing.Tests.Helpers;

public class AuditTrailValidatorTests
{
    [Fact]
    public void Validate_WithValidAudit_ReturnsValid()
    {
        // Arrange
        var audit = new AuditTrail
        {
            TableName = "Products",
            DateTimeUTC = DateTime.UtcNow,
            KeyValues = "{\"Id\":1}",
            ChangedColumns = "[\"Name\"]",
            OldValues = "{\"Name\":\"Old\"}",
            NewValues = "{\"Name\":\"New\"}"
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithNullTableName_ReturnsInvalid()
    {
        // Arrange
        var audit = new AuditTrail
        {
            TableName = null,
            DateTimeUTC = DateTime.UtcNow
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("TableName"));
    }

    [Fact]
    public void Validate_WithTooLongTableName_ReturnsInvalid()
    {
        // Arrange
        var audit = new AuditTrail
        {
            TableName = new string('A', AuditTrail.MaxTableNameLength + 1),
            DateTimeUTC = DateTime.UtcNow
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("TableName") && e.Contains("maximum length"));
    }

    [Fact]
    public void Validate_WithTooLongKeyValues_ReturnsInvalid()
    {
        // Arrange
        var audit = new AuditTrail
        {
            TableName = "Products",
            DateTimeUTC = DateTime.UtcNow,
            KeyValues = new string('A', AuditTrail.MaxKeyValuesLength + 1)
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("KeyValues") && e.Contains("maximum length"));
    }

    [Fact]
    public void Validate_WithTooLongChangedColumns_ReturnsInvalid()
    {
        // Arrange
        var audit = new AuditTrail
        {
            TableName = "Products",
            DateTimeUTC = DateTime.UtcNow,
            ChangedColumns = new string('A', AuditTrail.MaxChangedColumnsLength + 1)
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("ChangedColumns") && e.Contains("maximum length"));
    }

    [Fact]
    public void Validate_WithDefaultDateTime_ReturnsInvalid()
    {
        // Arrange
        var audit = new AuditTrail
        {
            TableName = "Products",
            DateTimeUTC = default
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("DateTimeUTC"));
    }

    [Fact]
    public void Validate_WithNonUtcDateTime_ReturnsWarning()
    {
        // Arrange
        var audit = new AuditTrail
        {
            TableName = "Products",
            DateTimeUTC = DateTime.Now
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Warnings, w => w.Contains("DateTimeUTC") && w.Contains("UTC"));
    }

    [Fact]
    public void Validate_WithInvalidArchiveAfter_ReturnsInvalid()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var audit = new AuditTrail
        {
            TableName = "Products",
            DateTimeUTC = now,
            ArchiveAfter = now.AddDays(-1)
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("ArchiveAfter"));
    }

    [Fact]
    public void Validate_WithInvalidJson_ReturnsInvalid()
    {
        // Arrange
        var audit = new AuditTrail
        {
            TableName = "Products",
            DateTimeUTC = DateTime.UtcNow,
            KeyValues = "invalid json"
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("KeyValues") && e.Contains("JSON"));
    }

    [Fact]
    public void WillExceedMaxLength_WithLongString_ReturnsTrue()
    {
        // Arrange
        var value = new string('A', 100);
        var maxLength = 50;

        // Act
        var result = AuditTrailValidator.WillExceedMaxLength(value, maxLength, out var actualLength);

        // Assert
        Assert.True(result);
        Assert.Equal(100, actualLength);
    }

    [Fact]
    public void WillExceedMaxLength_WithShortString_ReturnsFalse()
    {
        // Arrange
        var value = new string('A', 30);
        var maxLength = 50;

        // Act
        var result = AuditTrailValidator.WillExceedMaxLength(value, maxLength, out var actualLength);

        // Assert
        Assert.False(result);
        Assert.Equal(30, actualLength);
    }

    [Fact]
    public void WillExceedMaxLength_WithNull_ReturnsFalse()
    {
        // Arrange
        string? value = null;
        var maxLength = 50;

        // Act
        var result = AuditTrailValidator.WillExceedMaxLength(value, maxLength, out var actualLength);

        // Assert
        Assert.False(result);
        Assert.Equal(0, actualLength);
    }

    [Fact]
    public void TruncateIfNeeded_WithLongString_TruncatesCorrectly()
    {
        // Arrange
        var value = "This is a very long string that needs to be truncated";
        var maxLength = 20;

        // Act
        var result = AuditTrailValidator.TruncateIfNeeded(value, maxLength);

        // Assert
        Assert.Equal(20, result.Length);
        Assert.EndsWith("...", result);
    }

    [Fact]
    public void TruncateIfNeeded_WithShortString_ReturnsOriginal()
    {
        // Arrange
        var value = "Short string";
        var maxLength = 50;

        // Act
        var result = AuditTrailValidator.TruncateIfNeeded(value, maxLength);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void TruncateIfNeeded_WithNull_ReturnsEmpty()
    {
        // Arrange
        string? value = null;
        var maxLength = 50;

        // Act
        var result = AuditTrailValidator.TruncateIfNeeded(value, maxLength);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void TruncateIfNeeded_WithCustomSuffix_UsesSuffix()
    {
        // Arrange
        var value = "This is a very long string";
        var maxLength = 15;
        var suffix = "...more";

        // Act
        var result = AuditTrailValidator.TruncateIfNeeded(value, maxLength, suffix);

        // Assert
        Assert.Equal(15, result.Length);
        Assert.EndsWith(suffix, result);
    }

    [Fact]
    public void Validate_WithNullAudit_ThrowsArgumentNullException()
    {
        // Arrange
        AuditTrail audit = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AuditTrailValidator.Validate(audit));
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var audit = new AuditTrail
        {
            TableName = null,
            DateTimeUTC = default,
            KeyValues = "invalid json"
        };

        // Act
        var result = AuditTrailValidator.Validate(audit);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.HasErrors);
        Assert.True(result.Errors.Count >= 3);
    }
}
