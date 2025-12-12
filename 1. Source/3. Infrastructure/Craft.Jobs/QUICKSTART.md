# Craft.Jobs - Quick Start Guide

## 1-Minute Setup

### Step 1: Install PostgreSQL

Download and install PostgreSQL from https://www.postgresql.org/download/

Or use Docker:
```bash
docker run --name craftjobs-postgres -e POSTGRES_PASSWORD=your_password -p 5432:5432 -d postgres:17
```

### Step 2: Create Database

```sql
CREATE DATABASE CraftJobs;
```

That's it! Hangfire will create all tables automatically.

### Step 3: Configure

Add to `appsettings.json`:

```json
{
  "JobOptions": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CraftJobs;Username=postgres;Password=your_password"
  }
}
```

### Step 4: Register Services

In `Program.cs`:

```csharp
using Craft.Jobs;

// Add background jobs
builder.Services.AddJobServices(builder.Configuration);

// Register your jobs
builder.Services.AddScoped<YourJob>();

var app = builder.Build();

// Enable dashboard
app.UseJobDashboard();

app.Run();
```

### Step 5: Create a Job

```csharp
using Craft.Jobs;

public class WelcomeEmailJob : IBackgroundJob<WelcomeEmailJob.Parameters>
{
    private readonly ILogger<WelcomeEmailJob> _logger;

    public WelcomeEmailJob(ILogger<WelcomeEmailJob> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use ExecuteAsync with parameters");
    }

    public async Task ExecuteAsync(Parameters parameters, JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending welcome email to {Email}", parameters.Email);
        // Your email logic here
        await Task.CompletedTask;
    }

    public record Parameters(string Email, string Name);
}
```

### Step 6: Schedule the Job

```csharp
public class UserController : ControllerBase
{
    private readonly IJobScheduler _jobScheduler;

    public UserController(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    [HttpPost("register")]
    public IActionResult Register(string email, string name)
    {
        // Register user...

        // Schedule welcome email
        var jobId = _jobScheduler.Schedule<WelcomeEmailJob>(
            new WelcomeEmailJob.Parameters(email, name));

        return Ok(new { message = "User registered", jobId });
    }
}
```

### Step 7: View Dashboard

Navigate to: `https://localhost:5001/hangfire`

## Common Scenarios

### Schedule for Later

```csharp
// In 1 hour
_jobScheduler.Schedule<MyJob>(TimeSpan.FromHours(1), parameters);

// At specific time
_jobScheduler.Schedule<MyJob>(DateTimeOffset.UtcNow.AddDays(7), parameters);
```

### Recurring Jobs

```csharp
// Every day at 2 AM
_jobScheduler.ScheduleRecurring<DailyCleanupJob>(
    "daily-cleanup",
    CronExpressions.DailyAt2AM);

// Every 15 minutes
_jobScheduler.ScheduleRecurring<SyncJob>(
    "data-sync",
    intervalMinutes: 15);
```

### Multi-Tenancy

Enable in `appsettings.json`:

```json
{
  "JobOptions": {
    "EnableMultiTenancy": true
  }
}
```

Jobs automatically capture and restore tenant context!

## Production Checklist

- [ ] Use separate PostgreSQL database for jobs
- [ ] Secure dashboard (authentication required)
- [ ] Disable dashboard in production or protect it
- [ ] Set appropriate worker count (based on server capacity)
- [ ] Configure job expiration (default: 7 days)
- [ ] Use connection pooling
- [ ] Monitor database performance
- [ ] Set up backups for job database
- [ ] Test retry logic
- [ ] Enable SSL for database connections

## Need More?

See [README.md](./README.md) for complete documentation.

## Troubleshooting

**Jobs not running?**
- Check Hangfire server is started
- Verify database connection
- Check dashboard for errors

**Dashboard 404?**
- Ensure `UseJobDashboard()` is called
- Check dashboard is after `UseRouting()`
- Verify `EnableDashboard` is true

**Connection errors?**
- Verify PostgreSQL is running
- Check connection string
- Ensure database exists
- Check user permissions

## Examples

See sample jobs in `Craft.Jobs/Samples/`:
- `SendEmailJob.cs` - Email with parameters
- `DatabaseCleanupJob.cs` - Recurring job
- `GenerateReportJob.cs` - Complex parameters

Happy job scheduling! ??
