# Craft.Jobs - Background Job Processing

> **Version:** 1.0.0 | **Target Framework:** .NET 10

## ?? Table of Contents

1. [Overview](#overview)
2. [Features](#features)
3. [Quick Start](#quick-start)
4. [Configuration](#configuration)
5. [Creating Jobs](#creating-jobs)
6. [Scheduling Jobs](#scheduling-jobs)
7. [Multi-Tenancy Support](#multi-tenancy-support)
8. [Dashboard](#dashboard)
9. [Database Setup](#database-setup)
10. [Advanced Scenarios](#advanced-scenarios)
11. [Best Practices](#best-practices)
12. [Troubleshooting](#troubleshooting)
13. [API Reference](#api-reference)

---

## Overview

`Craft.Jobs` provides a robust, production-ready background job processing system built on **Hangfire** with **PostgreSQL** storage. It seamlessly integrates with the Craft framework, supporting dependency injection, multi-tenancy, and automatic retry mechanisms.

### Why Craft.Jobs?

- ? **Hangfire-Powered**: Industry-standard background job processing
- ? **PostgreSQL Storage**: Reliable, persistent job storage
- ? **Multi-Tenant**: Full support for tenant-isolated jobs
- ? **Dependency Injection**: All jobs run in scoped DI context
- ? **Automatic Retry**: Configurable retry policies
- ? **Dashboard**: Built-in web UI for monitoring
- ? **Cron Scheduling**: Flexible recurring job scheduling
- ? **Type-Safe**: Strongly-typed job parameters with JSON serialization

---

## Features

### ? Core Features

- ? **Fire-and-Forget Jobs**: Execute immediately in the background
- ? **Delayed Jobs**: Schedule for specific time or delay
- ? **Recurring Jobs**: Cron-based scheduling (minutes, hourly, daily, etc.)
- ? **Job Continuations**: Chain jobs together
- ? **Automatic Retry**: Failed jobs automatically retry
- ? **Job Cancellation**: Cancel pending jobs
- ? **Job Monitoring**: Real-time dashboard

### ? Craft Integration

- ? **Scoped Services**: Jobs execute in scoped DI container
- ? **Multi-Tenancy**: Automatic tenant context preservation
- ? **Current User**: Access user context in jobs
- ? **Logging**: Integrated with Microsoft.Extensions.Logging
- ? **Configuration**: Options pattern with validation

### ? PostgreSQL Features

- ? **Persistent Storage**: Survives application restarts
- ? **Scalability**: Supports multiple server instances
- ? **Schema Customization**: Custom schema names
- ? **Automatic Migration**: Schema created automatically

---

## Quick Start

### 1. Installation

The project is already added to your solution:
- **Project**: `Craft.Jobs`
- **Location**: `1. Source/3. Infrastructure/Craft.Jobs/`

### 2. Configuration

Add to your `appsettings.json`:

```json
{
  "JobOptions": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CraftJobs;Username=postgres;Password=your_password",
    "EnableDashboard": true,
    "DashboardPath": "/hangfire",
    "WorkerCount": 20,
    "MaxRetryAttempts": 3,
    "EnableAutomaticRetry": true,
    "JobExpirationDays": 7,
    "EnableMultiTenancy": false,
    "SchemaName": "hangfire",
    "PollingIntervalSeconds": 15,
    "EnableDetailedLogging": false
  }
}
```

### 3. Register Services

In `Program.cs`:

```csharp
using Craft.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add job services
builder.Services.AddJobServices(builder.Configuration);

// Register your jobs
builder.Services.AddScoped<SendEmailJob>();
builder.Services.AddScoped<DatabaseCleanupJob>();
builder.Services.AddScoped<GenerateReportJob>();

var app = builder.Build();

// Enable job dashboard (optional, but recommended)
app.UseJobDashboard();

app.Run();
```

### 4. Create Your First Job

```csharp
using Craft.Jobs;
using Microsoft.Extensions.Logging;

public class SendWelcomeEmailJob : IBackgroundJob<SendWelcomeEmailJob.Parameters>
{
    private readonly ILogger<SendWelcomeEmailJob> _logger;
    private readonly IEmailService _emailService;

    public SendWelcomeEmailJob(
        ILogger<SendWelcomeEmailJob> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use ExecuteAsync with parameters");
    }

    public async Task ExecuteAsync(
        Parameters parameters,
        JobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending welcome email to {Email}", parameters.Email);

        await _emailService.SendWelcomeEmailAsync(
            parameters.Email,
            parameters.Name,
            cancellationToken);

        _logger.LogInformation("Welcome email sent successfully");
    }

    public record Parameters(string Email, string Name);
}
```

### 5. Schedule the Job

```csharp
public class UserService
{
    private readonly IJobScheduler _jobScheduler;

    public UserService(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    public async Task RegisterUserAsync(string email, string name)
    {
        // ... user registration logic ...

        // Schedule welcome email
        var jobId = _jobScheduler.Schedule<SendWelcomeEmailJob>(
            new SendWelcomeEmailJob.Parameters(email, name));

        Console.WriteLine($"Welcome email job scheduled with ID: {jobId}");
    }
}
```

---

## Configuration

### JobOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | string | Required | PostgreSQL connection string |
| `EnableDashboard` | bool | true | Enable Hangfire dashboard |
| `DashboardPath` | string | "/hangfire" | Dashboard URL path |
| `WorkerCount` | int | 20 | Maximum concurrent jobs (1-100) |
| `MaxRetryAttempts` | int | 3 | Retry attempts for failed jobs (0-10) |
| `EnableAutomaticRetry` | bool | true | Auto-retry failed jobs |
| `JobExpirationDays` | int | 7 | Days to keep job history (1-365) |
| `EnableMultiTenancy` | bool | false | Enable tenant-aware jobs |
| `SchemaName` | string | "hangfire" | PostgreSQL schema name |
| `PollingIntervalSeconds` | int | 15 | Job polling interval (1-60) |
| `EnableDetailedLogging` | bool | false | Verbose logging |

### Connection String Examples

**Local PostgreSQL:**
```json
"ConnectionString": "Host=localhost;Port=5432;Database=CraftJobs;Username=postgres;Password=password"
```

**Azure PostgreSQL:**
```json
"ConnectionString": "Host=myserver.postgres.database.azure.com;Port=5432;Database=craftjobs;Username=admin@myserver;Password=password;SslMode=Require"
```

**AWS RDS PostgreSQL:**
```json
"ConnectionString": "Host=myinstance.region.rds.amazonaws.com;Port=5432;Database=craftjobs;Username=postgres;Password=password;SslMode=Require"
```

### Environment-Specific Configuration

**appsettings.Development.json:**
```json
{
  "JobOptions": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CraftJobs_Dev;Username=postgres;Password=dev",
    "EnableDashboard": true,
    "WorkerCount": 5,
    "EnableDetailedLogging": true
  }
}
```

**appsettings.Production.json:**
```json
{
  "JobOptions": {
    "ConnectionString": "Host=prod-db.example.com;Port=5432;Database=CraftJobs;Username=jobuser;Password=${JOB_DB_PASSWORD};SslMode=Require",
    "EnableDashboard": false,
    "WorkerCount": 50,
    "EnableDetailedLogging": false
  }
}
```

---

## Creating Jobs

### Simple Job (No Parameters)

```csharp
public class DatabaseBackupJob : IBackgroundJob
{
    private readonly ILogger<DatabaseBackupJob> _logger;
    private readonly IBackupService _backupService;

    public DatabaseBackupJob(
        ILogger<DatabaseBackupJob> logger,
        IBackupService backupService)
    {
        _logger = logger;
        _backupService = backupService;
    }

    public async Task ExecuteAsync(
        JobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database backup");

        await _backupService.CreateBackupAsync(cancellationToken);

        _logger.LogInformation("Database backup completed");
    }
}
```

### Parameterized Job

```csharp
public class ProcessOrderJob : IBackgroundJob<ProcessOrderJob.Parameters>
{
    private readonly ILogger<ProcessOrderJob> _logger;
    private readonly IOrderService _orderService;

    public ProcessOrderJob(
        ILogger<ProcessOrderJob> logger,
        IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use ExecuteAsync with parameters");
    }

    public async Task ExecuteAsync(
        Parameters parameters,
        JobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing order {OrderId}", parameters.OrderId);

        await _orderService.ProcessOrderAsync(
            parameters.OrderId,
            parameters.Priority,
            cancellationToken);

        _logger.LogInformation("Order {OrderId} processed successfully", parameters.OrderId);
    }

    public record Parameters(int OrderId, string Priority);
}
```

### Job with Complex Parameters

```csharp
public class SendBulkEmailJob : IBackgroundJob<SendBulkEmailJob.Parameters>
{
    private readonly ILogger<SendBulkEmailJob> _logger;
    private readonly IEmailService _emailService;

    public SendBulkEmailJob(
        ILogger<SendBulkEmailJob> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use ExecuteAsync with parameters");
    }

    public async Task ExecuteAsync(
        Parameters parameters,
        JobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending {Count} emails with template {Template}",
            parameters.Recipients.Count,
            parameters.TemplateName);

        foreach (var recipient in parameters.Recipients)
        {
            await _emailService.SendTemplateEmailAsync(
                recipient.Email,
                parameters.TemplateName,
                recipient.Data,
                cancellationToken);
        }

        _logger.LogInformation("Bulk email job completed");
    }

    public record Parameters(
        string TemplateName,
        List<Recipient> Recipients);

    public record Recipient(string Email, Dictionary<string, string> Data);
}
```

### Job Registration

Register all your jobs in `Program.cs`:

```csharp
// Register as scoped (recommended for most jobs)
builder.Services.AddScoped<SendWelcomeEmailJob>();
builder.Services.AddScoped<ProcessOrderJob>();
builder.Services.AddScoped<DatabaseBackupJob>();

// Or register as transient
builder.Services.AddTransient<SendBulkEmailJob>();

// Or register as singleton (for stateless jobs)
builder.Services.AddSingleton<LogCleanupJob>();
```

---

## Scheduling Jobs

### Fire-and-Forget (Immediate Execution)

```csharp
// Simple job
var jobId = _jobScheduler.Schedule<DatabaseBackupJob>();

// Job with parameters
var jobId = _jobScheduler.Schedule<SendWelcomeEmailJob>(
    new SendWelcomeEmailJob.Parameters("user@example.com", "John Doe"));
```

### Delayed Jobs

```csharp
// Delay by TimeSpan
var jobId = _jobScheduler.Schedule<SendReminderJob>(
    TimeSpan.FromHours(24),
    new SendReminderJob.Parameters("user@example.com", "Meeting tomorrow"));

// Delay to specific time
var scheduledTime = DateTimeOffset.UtcNow.AddDays(7);
var jobId = _jobScheduler.Schedule<SendFollowUpJob>(
    scheduledTime,
    new SendFollowUpJob.Parameters("user@example.com"));
```

### Recurring Jobs (Cron-based)

```csharp
// Daily at 2 AM
_jobScheduler.ScheduleRecurring<DatabaseCleanupJob>(
    "database-cleanup",
    CronExpressions.DailyAt2AM);

// Every 15 minutes
_jobScheduler.ScheduleRecurring<SyncDataJob>(
    "data-sync",
    CronExpressions.Every15Minutes);

// Custom cron expression (weekdays at 9 AM)
_jobScheduler.ScheduleRecurring<GenerateDailyReportJob>(
    "daily-report",
    "0 9 * * 1-5");

// With parameters
_jobScheduler.ScheduleRecurring<BackupJob>(
    "nightly-backup",
    CronExpressions.DailyAt2AM,
    new BackupJob.Parameters("full"));
```

### Recurring Jobs (Minute-based)

```csharp
// Every 5 minutes
_jobScheduler.ScheduleRecurring<HealthCheckJob>(
    "health-check",
    intervalMinutes: 5);

// Every 30 minutes
_jobScheduler.ScheduleRecurring<CacheRefreshJob>(
    "cache-refresh",
    intervalMinutes: 30);
```

### Managing Recurring Jobs

```csharp
// Trigger immediately (doesn't affect schedule)
_jobScheduler.TriggerRecurring("database-cleanup");

// Remove recurring job
_jobScheduler.RemoveRecurring("database-cleanup");
```

### Job Management

```csharp
// Delete a specific job
bool deleted = _jobScheduler.Delete(jobId);

// Requeue a failed job
bool requeued = _jobScheduler.Requeue(jobId);
```

### Cron Expression Helpers

```csharp
using Craft.Jobs;

// Built-in expressions
CronExpressions.EveryMinute          // "* * * * *"
CronExpressions.Every5Minutes        // "*/5 * * * *"
CronExpressions.Every15Minutes       // "*/15 * * * *"
CronExpressions.Every30Minutes       // "*/30 * * * *"
CronExpressions.Hourly               // "0 * * * *"
CronExpressions.Daily                // "0 0 * * *"
CronExpressions.DailyAt1AM           // "0 1 * * *"
CronExpressions.DailyAt2AM           // "0 2 * * *"
CronExpressions.Weekly               // "0 0 * * 0"
CronExpressions.Monthly              // "0 0 1 * *"
CronExpressions.WeekdaysAt9AM        // "0 9 * * 1-5"

// Dynamic expressions
CronExpressions.EveryNMinutes(10)    // "*/10 * * * *"
CronExpressions.EveryNHours(6)       // "0 */6 * * *"
CronExpressions.DailyAtHour(14, 30)  // "30 14 * * *" (2:30 PM)
```

---

## Multi-Tenancy Support

### Enable Multi-Tenancy

```json
{
  "JobOptions": {
    "EnableMultiTenancy": true
  }
}
```

### How It Works

When multi-tenancy is enabled:

1. **Tenant Context Captured**: When scheduling a job, the current tenant context is automatically captured
2. **Tenant Context Restored**: When executing the job, the tenant context is restored
3. **Job Isolation**: Jobs are tagged with tenant information for filtering and monitoring

### Example Multi-Tenant Job

```csharp
public class TenantReportJob : IBackgroundJob<TenantReportJob.Parameters>
{
    private readonly ILogger<TenantReportJob> _logger;
    private readonly ITenant _currentTenant;
    private readonly IReportService _reportService;

    public TenantReportJob(
        ILogger<TenantReportJob> logger,
        ITenant currentTenant,
        IReportService reportService)
    {
        _logger = logger;
        _currentTenant = currentTenant;
        _reportService = reportService;
    }

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use ExecuteAsync with parameters");
    }

    public async Task ExecuteAsync(
        Parameters parameters,
        JobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        // Tenant context is automatically available
        _logger.LogInformation(
            "Generating report for tenant {TenantId} ({TenantName})",
            context.TenantId,
            _currentTenant.Name);

        await _reportService.GenerateReportAsync(
            parameters.ReportType,
            context.TenantId!,
            cancellationToken);

        _logger.LogInformation("Tenant report generated successfully");
    }

    public record Parameters(string ReportType);
}
```

### Scheduling Tenant-Specific Jobs

```csharp
public class TenantService
{
    private readonly IJobScheduler _jobScheduler;
    private readonly ITenant _currentTenant;

    public TenantService(IJobScheduler jobScheduler, ITenant currentTenant)
    {
        _jobScheduler = jobScheduler;
        _currentTenant = currentTenant;
    }

    public void ScheduleDailyReport()
    {
        // Current tenant context is automatically captured
        _jobScheduler.ScheduleRecurring<TenantReportJob>(
            $"daily-report-{_currentTenant.Id}",
            CronExpressions.DailyAt2AM,
            new TenantReportJob.Parameters("daily-summary"));
    }
}
```

---

## Dashboard

### Accessing the Dashboard

If `EnableDashboard` is true, access at:
```
https://your-app.com/hangfire
```

### Dashboard Features

- ? **Real-time Monitoring**: View executing, scheduled, and failed jobs
- ? **Job Statistics**: Success/failure rates, processing times
- ? **Job Details**: Parameters, execution history, stack traces
- ? **Retry Management**: Manually retry failed jobs
- ? **Recurring Jobs**: View and trigger recurring jobs
- ? **Server Status**: Worker count, server health

### Dashboard Authorization

The default implementation requires authentication. Customize in `JobServiceExtensions.cs`:

```csharp
internal class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Example: Only allow authenticated administrators
        return httpContext.User?.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Administrator");

        // Or use a custom policy
        // return httpContext.User.HasClaim("Permission", "ViewDashboard");
    }
}
```

### Disable Dashboard in Production

```json
{
  "JobOptions": {
    "EnableDashboard": false
  }
}
```

---

## Database Setup

### Automatic Setup (Recommended)

Hangfire automatically creates the required schema and tables on first run when `PrepareSchemaIfNecessary` is true (default).

No manual database migration needed!

### Database Requirements

- **PostgreSQL**: Version 9.5 or higher
- **Permissions**: User must have permissions to:
  - Create schema (if doesn't exist)
  - Create tables, indexes, and sequences
  - Insert, update, delete, and select data

### Connection String Requirements

```json
{
  "JobOptions": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CraftJobs;Username=postgres;Password=password"
  }
}
```

**Required components:**
- `Host`: PostgreSQL server address
- `Port`: PostgreSQL port (default: 5432)
- `Database`: Database name
- `Username`: Database user
- `Password`: User password

**Optional components:**
- `SslMode`: SSL mode (Require, Prefer, Disable)
- `Timeout`: Connection timeout
- `CommandTimeout`: Command timeout
- `Pooling`: Connection pooling (default: true)

### Separate Job Database (Recommended)

**Option 1: Separate database on same server**

```json
{
  "JobOptions": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CraftJobs;Username=postgres;Password=password"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=CraftApp;Username=postgres;Password=password"
  }
}
```

**Option 2: Separate database server**

```json
{
  "JobOptions": {
    "ConnectionString": "Host=jobs-db.example.com;Port=5432;Database=CraftJobs;Username=jobuser;Password=password"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=app-db.example.com;Port=5432;Database=CraftApp;Username=appuser;Password=password"
  }
}
```

### Custom Schema Name

```json
{
  "JobOptions": {
    "SchemaName": "background_jobs"
  }
}
```

This creates tables in the `background_jobs` schema instead of `hangfire`.

### Database Tables

Hangfire creates these tables automatically:

- `hangfire.job` - Job definitions and metadata
- `hangfire.jobparameter` - Job parameters
- `hangfire.jobqueue` - Job queue
- `hangfire.server` - Server instances
- `hangfire.state` - Job state history
- `hangfire.list`, `hangfire.set`, `hangfire.hash`, `hangfire.counter` - Internal structures

### Database Security

**Create a dedicated user:**

```sql
-- Create database
CREATE DATABASE CraftJobs;

-- Create user
CREATE USER jobuser WITH PASSWORD 'secure_password';

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE CraftJobs TO jobuser;

-- Connect to CraftJobs database
\c CraftJobs

-- Grant schema permissions
GRANT ALL PRIVILEGES ON SCHEMA public TO jobuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO jobuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO jobuser;
```

**Using custom schema:**

```sql
-- Create schema
CREATE SCHEMA hangfire;

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA hangfire TO jobuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA hangfire TO jobuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA hangfire TO jobuser;
```

### Backup and Maintenance

**Backup job database:**

```bash
pg_dump -h localhost -U postgres -d CraftJobs > craftjobs_backup.sql
```

**Restore job database:**

```bash
psql -h localhost -U postgres -d CraftJobs < craftjobs_backup.sql
```

**Clean up old jobs:**

Hangfire automatically cleans up jobs based on `JobExpirationDays` setting. You can also manually clean up:

```sql
-- Delete succeeded jobs older than 7 days
DELETE FROM hangfire.job 
WHERE statename = 'Succeeded' 
AND createdat < NOW() - INTERVAL '7 days';
```

---

## Advanced Scenarios

### Job Continuations

Chain jobs together:

```csharp
// Schedule job B after job A completes
var jobAId = _jobScheduler.Schedule<ProcessOrderJob>(
    new ProcessOrderJob.Parameters(orderId, "high"));

BackgroundJob.ContinueJobWith<SendConfirmationEmailJob>(
    jobAId,
    job => job.ExecuteAsync(
        new SendConfirmationEmailJob.Parameters(orderId),
        null!,
        CancellationToken.None));
```

### Batch Jobs

Process multiple items:

```csharp
public class ProcessBatchJob : IBackgroundJob<ProcessBatchJob.Parameters>
{
    private readonly ILogger<ProcessBatchJob> _logger;
    private readonly IJobScheduler _jobScheduler;

    public ProcessBatchJob(
        ILogger<ProcessBatchJob> logger,
        IJobScheduler jobScheduler)
    {
        _logger = logger;
        _jobScheduler = jobScheduler;
    }

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use ExecuteAsync with parameters");
    }

    public async Task ExecuteAsync(
        Parameters parameters,
        JobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing batch of {Count} items", parameters.ItemIds.Count);

        // Schedule individual jobs for each item
        foreach (var itemId in parameters.ItemIds)
        {
            _jobScheduler.Schedule<ProcessItemJob>(
                new ProcessItemJob.Parameters(itemId));
        }

        _logger.LogInformation("Batch processing initiated");
    }

    public record Parameters(List<int> ItemIds);
}
```

### Long-Running Jobs

For jobs that take a long time:

```csharp
public class DataMigrationJob : IBackgroundJob
{
    private readonly ILogger<DataMigrationJob> _logger;
    private readonly IDataService _dataService;

    public DataMigrationJob(
        ILogger<DataMigrationJob> logger,
        IDataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    [AutomaticRetry(Attempts = 0)] // Disable retry for long-running jobs
    public async Task ExecuteAsync(
        JobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting data migration");

        var totalRecords = await _dataService.GetTotalRecordsAsync();
        var batchSize = 1000;
        var processed = 0;

        for (var i = 0; i < totalRecords; i += batchSize)
        {
            await _dataService.MigrateBatchAsync(i, batchSize, cancellationToken);
            processed += batchSize;

            _logger.LogInformation(
                "Migration progress: {Processed}/{Total} ({Percentage}%)",
                processed,
                totalRecords,
                (processed * 100) / totalRecords);
        }

        _logger.LogInformation("Data migration completed");
    }
}
```

### Job Filters

Custom job filters:

```csharp
public class LoggingJobFilter : IClientFilter, IServerFilter
{
    private readonly ILogger<LoggingJobFilter> _logger;

    public LoggingJobFilter(ILogger<LoggingJobFilter> logger)
    {
        _logger = logger;
    }

    public void OnCreating(CreatingContext filterContext)
    {
        _logger.LogInformation("Creating job: {JobType}", filterContext.Job.Type.Name);
    }

    public void OnCreated(CreatedContext filterContext)
    {
        _logger.LogInformation("Job created with ID: {JobId}", filterContext.BackgroundJob.Id);
    }

    public void OnPerforming(PerformingContext filterContext)
    {
        _logger.LogInformation("Executing job: {JobId}", filterContext.BackgroundJob.Id);
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        _logger.LogInformation("Job completed: {JobId}", filterContext.BackgroundJob.Id);
    }
}

// Register the filter
builder.Services.AddHangfire(config =>
{
    config.UseFilter(new LoggingJobFilter(logger));
});
```

### Accessing Job Context

```csharp
public class ContextAwareJob : IBackgroundJob
{
    private readonly ILogger<ContextAwareJob> _logger;

    public ContextAwareJob(ILogger<ContextAwareJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(
        JobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Job ID: {JobId}, Type: {JobType}, Attempt: {Attempt}",
            context.JobId,
            context.JobType,
            context.RetryAttempt);

        if (context.TenantId != null)
        {
            _logger.LogInformation("Executing for tenant: {TenantId}", context.TenantId);
        }

        if (context.UserId != null)
        {
            _logger.LogInformation("Scheduled by user: {UserId}", context.UserId);
        }

        // Access metadata
        foreach (var (key, value) in context.Metadata)
        {
            _logger.LogInformation("Metadata: {Key} = {Value}", key, value);
        }

        await Task.CompletedTask;
    }
}
```

---

## Best Practices

### ? Job Design

1. **Keep Jobs Small**: Each job should do one thing
2. **Make Jobs Idempotent**: Jobs should be safe to run multiple times
3. **Use Parameters**: Pass all required data as parameters
4. **Avoid Shared State**: Don't rely on static variables or singletons
5. **Handle Cancellation**: Always respect `CancellationToken`

```csharp
// ? Good: Small, focused, idempotent
public class SendEmailJob : IBackgroundJob<SendEmailJob.Parameters>
{
    public async Task ExecuteAsync(Parameters parameters, JobExecutionContext context, CancellationToken cancellationToken)
    {
        // Check if already sent (idempotency)
        if (await _emailService.IsAlreadySentAsync(parameters.EmailId))
        {
            _logger.LogInformation("Email {EmailId} already sent, skipping", parameters.EmailId);
            return;
        }

        await _emailService.SendAsync(parameters.EmailId, cancellationToken);
    }

    public record Parameters(string EmailId);
}

// ? Bad: Does too much, not idempotent
public class ProcessEverythingJob : IBackgroundJob
{
    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        // ? Too many responsibilities
        await SendEmails();
        await GenerateReports();
        await CleanupFiles();
        await UpdateStatistics();
        
        // ? Not idempotent - running twice causes issues
    }
}
```

### ? Error Handling

1. **Let Exceptions Bubble**: Hangfire will catch and retry
2. **Use Retry for Transient Errors**: Network issues, timeouts
3. **Disable Retry for Permanent Errors**: Invalid data, business logic errors
4. **Log Errors**: Always log what went wrong

```csharp
public class ReliableJob : IBackgroundJob
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        try
        {
            await _service.ProcessAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            // Business logic error - don't retry
            _logger.LogError(ex, "Invalid operation, not retrying");
            throw new InvalidOperationException("Business logic error - manual intervention required", ex);
        }
        catch (HttpRequestException ex)
        {
            // Network error - let retry handle it
            _logger.LogWarning(ex, "Network error, will retry");
            throw;
        }
    }
}
```

### ? Performance

1. **Use Scoped Services**: Jobs run in scoped context
2. **Batch Operations**: Process multiple items efficiently
3. **Optimize Worker Count**: Match to server capacity
4. **Use Appropriate Polling Interval**: Balance responsiveness vs database load

```csharp
// ? Good: Efficient batch processing
public class ProcessOrdersJob : IBackgroundJob
{
    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        const int batchSize = 100;
        var orders = await _orderService.GetPendingOrdersAsync(batchSize, cancellationToken);

        await _orderService.ProcessBatchAsync(orders, cancellationToken);
    }
}

// ? Bad: Processing one at a time
public class ProcessSingleOrderJob : IBackgroundJob<ProcessSingleOrderJob.Parameters>
{
    public async Task ExecuteAsync(Parameters parameters, JobExecutionContext context, CancellationToken cancellationToken)
    {
        // ? Inefficient - creates too many jobs
        await _orderService.ProcessOrderAsync(parameters.OrderId, cancellationToken);
    }
    
    public record Parameters(int OrderId);
}
```

### ? Testing

1. **Unit Test Job Logic**: Test without Hangfire
2. **Mock Dependencies**: Use DI for testability
3. **Test Error Scenarios**: Verify retry logic

```csharp
public class SendEmailJobTests
{
    [Fact]
    public async Task ExecuteAsync_SendsEmail_Successfully()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SendEmailJob>>();
        var emailServiceMock = new Mock<IEmailService>();
        var job = new SendEmailJob(loggerMock.Object, emailServiceMock.Object);
        var parameters = new SendEmailJob.Parameters("test@example.com", "Test");
        var context = new JobExecutionContext { JobId = "test-job" };

        // Act
        await job.ExecuteAsync(parameters, context, CancellationToken.None);

        // Assert
        emailServiceMock.Verify(
            x => x.SendAsync("test@example.com", "Test", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### ? Security

1. **Secure Dashboard**: Require authentication
2. **Encrypt Connection Strings**: Use secrets management
3. **Validate Parameters**: Don't trust job parameters
4. **Audit Sensitive Operations**: Log who scheduled what

```csharp
// ? Secure dashboard
public class SecureDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User?.IsInRole("Administrator") == true;
    }
}

// ? Validate parameters
public class DeleteUserJob : IBackgroundJob<DeleteUserJob.Parameters>
{
    public async Task ExecuteAsync(Parameters parameters, JobExecutionContext context, CancellationToken cancellationToken)
    {
        // ? Validate input
        if (parameters.UserId <= 0)
            throw new ArgumentException("Invalid user ID");

        // ? Check permissions
        if (context.UserId == parameters.UserId.ToString())
            throw new InvalidOperationException("Cannot delete your own account");

        // ? Audit
        _logger.LogWarning(
            "User {DeletedBy} is deleting user {UserId}",
            context.UserId,
            parameters.UserId);

        await _userService.DeleteAsync(parameters.UserId, cancellationToken);
    }

    public record Parameters(int UserId);
}
```

---

## Troubleshooting

### Issue: Jobs not executing

**Causes:**
- Hangfire server not running
- Database connection failed
- No workers available

**Solutions:**
1. Check Hangfire server is registered:
   ```csharp
   builder.Services.AddHangfireServer();
   ```
2. Verify database connection
3. Check dashboard for server status
4. Review logs for errors

---

### Issue: "Connection string is required"

**Cause:** Missing or invalid connection string

**Solution:**
```json
{
  "JobOptions": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CraftJobs;Username=postgres;Password=password"
  }
}
```

---

### Issue: Jobs stuck in "Enqueued" state

**Causes:**
- Worker count too low
- Jobs taking too long
- Database performance issues

**Solutions:**
1. Increase worker count:
   ```json
   {
     "JobOptions": {
       "WorkerCount": 50
     }
   }
   ```
2. Optimize job logic
3. Check database performance
4. Monitor dashboard for bottlenecks

---

### Issue: Dashboard returns 404

**Causes:**
- Dashboard not enabled
- Wrong path
- Middleware order

**Solutions:**
1. Enable dashboard:
   ```json
   {
     "JobOptions": {
       "EnableDashboard": true
     }
   }
   ```
2. Use dashboard after routing:
   ```csharp
   app.UseRouting();
   app.UseAuthentication();
   app.UseAuthorization();
   app.UseJobDashboard();
   ```
3. Check path: `/hangfire`

---

### Issue: Multi-tenancy not working

**Cause:** Multi-tenancy not enabled

**Solution:**
```json
{
  "JobOptions": {
    "EnableMultiTenancy": true
  }
}
```

Ensure `ITenant` is properly configured in your application.

---

### Issue: Database schema creation failed

**Causes:**
- Insufficient permissions
- Database doesn't exist

**Solutions:**
1. Grant schema permissions:
   ```sql
   GRANT CREATE ON DATABASE CraftJobs TO jobuser;
   ```
2. Create database manually:
   ```sql
   CREATE DATABASE CraftJobs;
   ```
3. Check connection string has correct credentials

---

### Issue: High database load

**Causes:**
- Polling interval too low
- Too many workers
- Complex queries

**Solutions:**
1. Increase polling interval:
   ```json
   {
     "JobOptions": {
       "PollingIntervalSeconds": 30
     }
   }
   ```
2. Reduce worker count
3. Optimize job queries
4. Use database connection pooling

---

## API Reference

### IBackgroundJob

```csharp
public interface IBackgroundJob
{
    Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
}
```

### IBackgroundJob<TParameters>

```csharp
public interface IBackgroundJob<in TParameters> : IBackgroundJob where TParameters : class
{
    Task ExecuteAsync(TParameters parameters, JobExecutionContext context, CancellationToken cancellationToken = default);
}
```

### IJobScheduler

```csharp
public interface IJobScheduler
{
    // Fire-and-forget
    string Schedule<TJob>(object? parameters = null) where TJob : IBackgroundJob;

    // Delayed
    string Schedule<TJob>(TimeSpan delay, object? parameters = null) where TJob : IBackgroundJob;
    string Schedule<TJob>(DateTimeOffset scheduledFor, object? parameters = null) where TJob : IBackgroundJob;

    // Recurring
    void ScheduleRecurring<TJob>(string recurringJobId, string cronExpression, object? parameters = null) where TJob : IBackgroundJob;
    void ScheduleRecurring<TJob>(string recurringJobId, int intervalMinutes, object? parameters = null) where TJob : IBackgroundJob;

    // Management
    void RemoveRecurring(string recurringJobId);
    void TriggerRecurring(string recurringJobId);
    bool Delete(string jobId);
    bool Requeue(string jobId);
}
```

### JobExecutionContext

```csharp
public class JobExecutionContext
{
    public string JobId { get; set; }
    public string JobType { get; set; }
    public int RetryAttempt { get; set; }
    public string? TenantId { get; set; }
    public string? UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}
```

### Extension Methods

```csharp
// Service registration
IServiceCollection AddJobServices(IConfiguration configuration);
IServiceCollection AddJobServices(IConfigurationSection configurationSection);
IServiceCollection AddJobServices(Action<JobOptions> configureOptions);

// Application builder
IApplicationBuilder UseJobDashboard();
```

---

## Summary

`Craft.Jobs` provides enterprise-grade background job processing with:

? **Hangfire Integration**: Industry-standard job processing  
? **PostgreSQL Storage**: Reliable, persistent job storage  
? **Multi-Tenancy**: Full tenant isolation support  
? **Scoped DI**: All jobs run in scoped service context  
? **Automatic Retry**: Configurable retry policies  
? **Dashboard**: Web-based monitoring and management  
? **Cron Scheduling**: Flexible recurring job scheduling  
? **Type-Safe**: Strongly-typed parameters with JSON serialization  

Perfect for:
- Email sending
- Report generation
- Data synchronization
- Database cleanup
- File processing
- Scheduled tasks

---

**Version**: 1.0.0  
**Last Updated**: January 2025  
**Target Framework**: .NET 10  
**Status**: ? Production Ready

---

**Need help?** Check the [Troubleshooting](#troubleshooting) section or enable detailed logging:

```json
{
  "JobOptions": {
    "EnableDetailedLogging": true
  }
}
```
