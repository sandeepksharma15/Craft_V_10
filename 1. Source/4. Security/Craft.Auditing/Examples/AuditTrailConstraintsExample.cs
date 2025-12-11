using Craft.Domain;

namespace Craft.Auditing.Examples;

/// <summary>
/// Example demonstrating the database column length constraints in AuditTrail.
/// </summary>
public static class AuditTrailConstraintsExample
{
    /// <summary>
    /// Public constants defining maximum lengths for audit trail fields.
    /// These ensure database compatibility and prevent truncation errors.
    /// </summary>
    public static class FieldConstraints
    {
        /// <summary>
        /// Maximum length for ChangedColumns JSON field.
        /// Stores: ["ColumnName1", "ColumnName2", ...]
        /// </summary>
        public const int ChangedColumns = AuditTrail.MaxChangedColumnsLength; // 2000

        /// <summary>
        /// Maximum length for KeyValues JSON field.
        /// Stores: {"Id": 123, "TenantId": 456}
        /// </summary>
        public const int KeyValues = AuditTrail.MaxKeyValuesLength; // 1000

        /// <summary>
        /// Maximum length for TableName field.
        /// Stores: Entity/table display name
        /// </summary>
        public const int TableName = AuditTrail.MaxTableNameLength; // 255

        /// <summary>
        /// OldValues and NewValues use nvarchar(max) to accommodate large entities.
        /// These can store complete JSON representations of entity state.
        /// </summary>
        public const string UnboundedFields = "OldValues, NewValues use nvarchar(max)";
    }

    /// <summary>
    /// Example showing typical audit trail data sizes.
    /// </summary>
    public class TypicalDataSizes
    {
        // Small entity example
        public class SmallEntityAudit
        {
            // ChangedColumns: ~50 bytes
            // ["Name", "Email", "PhoneNumber"]
            
            // KeyValues: ~20 bytes
            // {"Id":123}
            
            // OldValues: ~100 bytes
            // {"Name":"John","Email":"john@example.com","PhoneNumber":"555-1234"}
            
            // NewValues: ~100 bytes
            // {"Name":"Jane","Email":"jane@example.com","PhoneNumber":"555-5678"}
            
            // TableName: ~20 bytes
            // "Customer"
        }

        // Large entity example
        public class LargeEntityAudit
        {
            // ChangedColumns: ~500 bytes
            // ["Field1", "Field2", ... "Field50"]
            
            // KeyValues: ~50 bytes
            // {"Id":123,"TenantId":456,"PartitionKey":"ABC"}
            
            // OldValues: ~5000 bytes
            // Large JSON with 50+ properties
            
            // NewValues: ~5000 bytes
            // Large JSON with 50+ properties
            
            // TableName: ~50 bytes
            // "ComplexBusinessEntity"
        }
    }

    /// <summary>
    /// Example entity configurations that work well with audit trail constraints.
    /// </summary>
    public static class BestPractices
    {
        // ? GOOD: Entity with reasonable number of properties
        [Audit]
        public class WellDesignedEntity : BaseEntity
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            
            // Audit trail will easily fit within constraints
        }

        // ? GOOD: Exclude large properties from audit
        [Audit]
        public class EntityWithLargeData : BaseEntity
        {
            public string Name { get; set; } = string.Empty;
            
            [DoNotAudit]  // Exclude from audit trail
            public string LargeJsonBlob { get; set; } = string.Empty;
            
            [DoNotAudit]  // Exclude from audit trail
            public byte[] LargeFileData { get; set; } = [];
        }

        // ?? CAUTION: Very wide entity (many properties)
        [Audit]
        public class VeryWideEntity : BaseEntity
        {
            // If you have 100+ properties being tracked,
            // consider using [DoNotAudit] on less critical fields
            public string Field1 { get; set; } = string.Empty;
            // ... Field2 through Field100 ...
            
            [DoNotAudit]
            public string LessCriticalField { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// Example showing how constraints are enforced by the database.
    /// </summary>
    public class DatabaseBehavior
    {
        /// <summary>
        /// When using EF Core migrations, the database schema will be:
        /// 
        /// CREATE TABLE HK_AuditTrail (
        ///     Id bigint NOT NULL IDENTITY,
        ///     ChangedColumns nvarchar(2000),      -- MaxLength enforced
        ///     KeyValues nvarchar(1000),           -- MaxLength enforced
        ///     TableName nvarchar(255),            -- MaxLength enforced
        ///     OldValues nvarchar(max),            -- Unbounded
        ///     NewValues nvarchar(max),            -- Unbounded
        ///     ChangeType int NOT NULL,
        ///     DateTimeUTC datetime2 NOT NULL,
        ///     ShowDetails bit NOT NULL,
        ///     UserId bigint NOT NULL,
        ///     IsDeleted bit NOT NULL,
        ///     ConcurrencyStamp nvarchar(max),
        ///     CONSTRAINT PK_HK_AuditTrail PRIMARY KEY (Id)
        /// );
        /// 
        /// Indexes (from AuditTrailFeature.ConfigureModel):
        /// - IX_HK_AuditTrail_TableName
        /// - IX_HK_AuditTrail_DateTimeUTC
        /// - IX_HK_AuditTrail_UserId
        /// - IX_HK_AuditTrail_ChangeType
        /// </summary>
        public const string ExpectedSchema = "See above";
    }

    /// <summary>
    /// Example of monitoring and handling potential constraint violations.
    /// </summary>
    public static class MonitoringGuidance
    {
        /// <summary>
        /// If you encounter constraint violations, consider:
        /// 
        /// 1. Use [DoNotAudit] on properties with large values
        /// 2. Increase MaxChangedColumnsLength if you have very wide tables
        /// 3. Increase MaxKeyValuesLength if you have composite keys with many columns
        /// 4. Review your entity design - very wide entities may indicate design issues
        /// 5. OldValues and NewValues are already unbounded (nvarchar(max))
        /// </summary>
        public static void HandleConstraintViolations()
        {
            // Log when approaching limits
            var changedColumnsLength = 1800; // Approaching 2000 limit
            if (changedColumnsLength > AuditTrail.MaxChangedColumnsLength * 0.9)
            {
                // Log warning: Entity has too many tracked properties
                // Consider using [DoNotAudit] on some fields
            }
        }
    }
}
