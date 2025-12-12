# Database Migration Guide - Craft.Notifications

## Quick Setup

### 1. Update Your DbContext

Add the notification entities to your DbContext:

```csharp
using Craft.Notifications;
using Microsoft.EntityFrameworkCore;

public class YourDbContext : DbContext
{
    // Add DbSets for notification entities
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationDeliveryLog> NotificationDeliveryLogs { get; set; } = null!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

    // Your existing DbSets...
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure notification entities (adds all indexes and relationships)
        modelBuilder.ConfigureNotificationEntities();

        // Your other configurations...
    }
}
```

### 2. Create Migration

```bash
# Navigate to your project directory
cd "path/to/your/project"

# Create the migration
dotnet ef migrations add AddNotificationSystem --context YourDbContext

# Or if you have multiple startup projects:
dotnet ef migrations add AddNotificationSystem --context YourDbContext --startup-project "../YourStartupProject"
```

### 3. Review Migration

The generated migration will create three tables:

**Notifications** table:
- Id (Primary Key)
- Title, Message, Type, Priority, Status, Channels
- RecipientUserId, RecipientEmail, RecipientPhone
- SenderUserId, TenantId
- Metadata (JSON), ActionUrl, ImageUrl
- Category, ExpiresAt, ScheduledFor, DeliveredAt, ReadAt
- DeliveryAttempts, ErrorMessage
- CreatedAt, ModifiedAt, IsDeleted, DeletedAt

**NotificationDeliveryLogs** table:
- Id (Primary Key)
- NotificationId (Foreign Key)
- Channel, IsSuccess, ErrorMessage, ProviderResponse
- AttemptNumber, DurationMs
- CreatedAt

**NotificationPreferences** table:
- Id (Primary Key)
- UserId, TenantId, Category
- EnabledChannels, IsEnabled, MinimumPriority
- Email, Phone
- PushEndpoint, PushPublicKey, PushAuth
- WebhookUrl
- CreatedAt, ModifiedAt

**Indexes** (automatically created):
- `IX_Notifications_RecipientUserId`
- `IX_Notifications_Status`
- `IX_Notifications_CreatedAt`
- `IX_Notifications_RecipientUserId_ReadAt` (composite)
- `IX_Notifications_ScheduledFor`
- `IX_NotificationDeliveryLogs_NotificationId`
- `IX_NotificationDeliveryLogs_NotificationId_Channel` (composite)
- `IX_NotificationPreferences_UserId_Category` (unique composite)
- `IX_NotificationPreferences_UserId`

### 4. Update Database

```bash
# Apply the migration to your database
dotnet ef database update --context YourDbContext

# Or with specific connection string (for production)
dotnet ef database update --context YourDbContext --connection "Server=...;Database=...;Trusted_Connection=True;"
```

### 5. Verify Tables

Run this SQL to verify tables were created:

```sql
-- Check if tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Notifications', 'NotificationDeliveryLogs', 'NotificationPreferences')
ORDER BY TABLE_NAME;

-- Check Notifications table structure
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Notifications'
ORDER BY ORDINAL_POSITION;

-- Check indexes
SELECT i.name AS IndexName, c.name AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('Notifications', 'NotificationDeliveryLogs', 'NotificationPreferences')
ORDER BY t.name, i.name, ic.key_ordinal;
```

## Rollback (if needed)

```bash
# Remove the migration
dotnet ef migrations remove --context YourDbContext

# Or rollback to previous migration
dotnet ef database update PreviousMigrationName --context YourDbContext
```

## Sample Data (Optional)

```sql
-- Insert a sample notification for testing
INSERT INTO Notifications (
    Title, Message, [Type], Priority, Channels, [Status],
    RecipientUserId, CreatedAt, ExpiresAt, IsDeleted
) VALUES (
    'Welcome to the System',
    'Thank you for registering!',
    0, -- Info
    1, -- Normal
    1, -- InApp
    3, -- Delivered
    'user123',
    GETUTCDATE(),
    DATEADD(DAY, 30, GETUTCDATE()),
    0
);

-- Insert sample preference
INSERT INTO NotificationPreferences (
    UserId, EnabledChannels, IsEnabled, MinimumPriority, CreatedAt
) VALUES (
    'user123',
    31, -- All channels (binary: 11111)
    1,  -- Enabled
    0,  -- Low priority minimum
    GETUTCDATE()
);
```

## Multi-Database Support

### SQL Server (Default)
```csharp
services.AddDbContext<YourDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```

### PostgreSQL
```csharp
services.AddDbContext<YourDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
```

### SQLite
```csharp
services.AddDbContext<YourDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
```

### MySQL
```csharp
services.AddDbContext<YourDbContext>(options =>
    options.UseMySql(
        configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))));
```

## Production Considerations

### 1. Connection String Security
```json
// Use environment variables or Azure Key Vault in production
{
  "ConnectionStrings": {
    "DefaultConnection": "#{ConnectionString}#" // Token replacement in CI/CD
  }
}
```

### 2. Migration Strategy

**Blue-Green Deployment:**
```bash
# Test migration in staging
dotnet ef database update --context YourDbContext --connection "staging-connection-string"

# If successful, apply to production
dotnet ef database update --context YourDbContext --connection "production-connection-string"
```

**Zero-Downtime Migration:**
1. Add new tables (non-breaking)
2. Deploy application code
3. Migrate data (if needed)
4. Remove old tables (in next release)

### 3. Backup Before Migration
```sql
-- SQL Server backup
BACKUP DATABASE [YourDatabase] 
TO DISK = 'C:\Backups\YourDatabase_PreNotifications.bak'
WITH FORMAT, NAME = 'Pre-Notifications Migration Backup';

-- PostgreSQL backup
pg_dump -U username -d database_name -f backup.sql
```

### 4. Index Maintenance
```sql
-- Rebuild indexes periodically (SQL Server)
ALTER INDEX ALL ON Notifications REBUILD;
ALTER INDEX ALL ON NotificationDeliveryLogs REBUILD;
ALTER INDEX ALL ON NotificationPreferences REBUILD;

-- Update statistics
UPDATE STATISTICS Notifications;
UPDATE STATISTICS NotificationDeliveryLogs;
UPDATE STATISTICS NotificationPreferences;
```

## Troubleshooting

### Migration Fails with "Object already exists"
```bash
# Check for partially applied migrations
dotnet ef migrations list --context YourDbContext

# Force remove and re-add
dotnet ef database update 0 --context YourDbContext
dotnet ef migrations remove --context YourDbContext
dotnet ef migrations add AddNotificationSystem --context YourDbContext
dotnet ef database update --context YourDbContext
```

### Foreign Key Constraint Errors
Ensure your DbContext has cascade delete configured:
```csharp
modelBuilder.Entity<Notification>()
    .HasMany(e => e.DeliveryLogs)
    .WithOne(e => e.Notification)
    .HasForeignKey(e => e.NotificationId)
    .OnDelete(DeleteBehavior.Cascade); // Already configured by ConfigureNotificationEntities()
```

### Index Creation Fails
If you have existing data that violates unique constraints:
```sql
-- Find duplicate UserId + Category combinations
SELECT UserId, Category, COUNT(*) as Count
FROM NotificationPreferences
GROUP BY UserId, Category
HAVING COUNT(*) > 1;

-- Remove duplicates, keeping the most recent
WITH CTE AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY UserId, Category ORDER BY CreatedAt DESC) as RN
    FROM NotificationPreferences
)
DELETE FROM CTE WHERE RN > 1;
```

## Performance Tuning

### Add Additional Indexes (if needed)
```sql
-- For frequent queries by date range
CREATE NONCLUSTERED INDEX IX_Notifications_CreatedAt_Status
ON Notifications(CreatedAt, Status) INCLUDE (RecipientUserId, Title, Message);

-- For user notification lists
CREATE NONCLUSTERED INDEX IX_Notifications_RecipientUserId_CreatedAt
ON Notifications(RecipientUserId, CreatedAt DESC) 
WHERE IsDeleted = 0;

-- For delivery logs analysis
CREATE NONCLUSTERED INDEX IX_NotificationDeliveryLogs_Channel_IsSuccess
ON NotificationDeliveryLogs(Channel, IsSuccess) INCLUDE (DurationMs, CreatedAt);
```

### Partitioning (for high volume)
```sql
-- Partition Notifications by CreatedAt (monthly)
CREATE PARTITION FUNCTION PF_NotificationsByMonth (DATETIME2)
AS RANGE RIGHT FOR VALUES (
    '2024-01-01', '2024-02-01', '2024-03-01', /* ... */
);

CREATE PARTITION SCHEME PS_NotificationsByMonth
AS PARTITION PF_NotificationsByMonth
ALL TO ([PRIMARY]);

-- Create clustered index on partition scheme
CREATE CLUSTERED INDEX CIX_Notifications_CreatedAt
ON Notifications(CreatedAt)
ON PS_NotificationsByMonth(CreatedAt);
```

## Cleanup Script (for testing)

```sql
-- WARNING: This deletes all notification data
DELETE FROM NotificationDeliveryLogs;
DELETE FROM Notifications;
DELETE FROM NotificationPreferences;

-- Reset identity seeds (SQL Server)
DBCC CHECKIDENT ('NotificationDeliveryLogs', RESEED, 0);
DBCC CHECKIDENT ('Notifications', RESEED, 0);
DBCC CHECKIDENT ('NotificationPreferences', RESEED, 0);
```

## Next Steps

1. ? Run migration
2. ? Verify tables created
3. ? Test with sample data
4. ? Configure notification options in appsettings.json
5. ? Start sending notifications!

See the main [README.md](README.md) for usage examples and configuration options.
