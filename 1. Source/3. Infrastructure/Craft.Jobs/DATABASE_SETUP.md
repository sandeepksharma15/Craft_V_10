# Craft.Jobs - Database Setup Guide

## Overview

`Craft.Jobs` uses **PostgreSQL** as the storage backend for **Hangfire**. This guide explains database setup, schema management, and production deployment.

---

## Automatic Setup (Recommended)

**Good news!** You don't need to run any migrations manually. Hangfire automatically creates all required tables and schema on the first application startup.

### Requirements

1. **PostgreSQL 9.5+** installed and running
2. **Database created** (can be empty)
3. **User has permissions** to create schema and tables

### Steps

1. **Create Database**
   ```sql
   CREATE DATABASE CraftJobs;
   ```

2. **Configure Connection String**
   ```json
   {
     "JobOptions": {
       "ConnectionString": "Host=localhost;Port=5432;Database=CraftJobs;Username=postgres;Password=your_password"
     }
   }
   ```

3. **Run Application**
   ```bash
   dotnet run
   ```

4. **Done!** Hangfire creates all tables automatically.

---

## Database Structure

### Schema

Default schema: `hangfire`

Can be customized:
```json
{
  "JobOptions": {
    "SchemaName": "background_jobs"
  }
}
```

### Tables Created

Hangfire automatically creates these tables:

| Table | Purpose |
|-------|---------|
| `hangfire.job` | Job definitions and metadata |
| `hangfire.jobparameter` | Job parameters (JSON) |
| `hangfire.jobqueue` | Job queue |
| `hangfire.server` | Server instances |
| `hangfire.state` | Job state history |
| `hangfire.list` | Internal list storage |
| `hangfire.set` | Internal set storage |
| `hangfire.hash` | Internal hash storage |
| `hangfire.counter` | Internal counters |
| `hangfire.aggregatedcounter` | Aggregated counters |
| `hangfire.lock` | Distributed locks |

---

## Production Setup

### Option 1: Separate Database (Recommended)

Use a dedicated database for background jobs:

```json
{
  "JobOptions": {
    "ConnectionString": "Host=jobs-db.example.com;Port=5432;Database=CraftJobs;Username=jobuser;Password=secure_password;SslMode=Require"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=app-db.example.com;Port=5432;Database=CraftApp;Username=appuser;Password=secure_password;SslMode=Require"
  }
}
```

**Benefits:**
- ? Isolate job processing from application data
- ? Independent scaling
- ? Easier backup/restore
- ? Better performance monitoring

### Option 2: Same Database, Different Schema

Use same database but separate schema:

```json
{
  "JobOptions": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CraftApp;Username=postgres;Password=password",
    "SchemaName": "jobs"
  }
}
```

**Benefits:**
- ? Single database to manage
- ? Simplified backups
- ? Schema isolation

---

## User Permissions

### Minimal Permissions

Create a dedicated user with minimal permissions:

```sql
-- Create user
CREATE USER jobuser WITH PASSWORD 'secure_password';

-- Grant database connection
GRANT CONNECT ON DATABASE CraftJobs TO jobuser;

-- Connect to the database
\c CraftJobs

-- Create schema (if custom)
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Grant schema permissions
GRANT CREATE ON SCHEMA hangfire TO jobuser;
GRANT ALL PRIVILEGES ON SCHEMA hangfire TO jobuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA hangfire TO jobuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA hangfire TO jobuser;

-- Future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA hangfire 
  GRANT ALL PRIVILEGES ON TABLES TO jobuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA hangfire 
  GRANT ALL PRIVILEGES ON SEQUENCES TO jobuser;
```

### Production Permissions (Restricted)

For production, limit permissions after initial setup:

```sql
-- After initial setup, remove CREATE permission
REVOKE CREATE ON SCHEMA hangfire FROM jobuser;

-- Grant only necessary permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA hangfire TO jobuser;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA hangfire TO jobuser;
```

---

## Cloud Providers

### Azure Database for PostgreSQL

```json
{
  "JobOptions": {
    "ConnectionString": "Host=myserver.postgres.database.azure.com;Port=5432;Database=craftjobs;Username=admin@myserver;Password=password;SslMode=Require"
  }
}
```

**Setup:**
1. Create Azure Database for PostgreSQL
2. Configure firewall rules
3. Enable SSL/TLS
4. Create database
5. Configure connection string

### AWS RDS PostgreSQL

```json
{
  "JobOptions": {
    "ConnectionString": "Host=myinstance.region.rds.amazonaws.com;Port=5432;Database=craftjobs;Username=postgres;Password=password;SslMode=Require"
  }
}
```

**Setup:**
1. Create RDS PostgreSQL instance
2. Configure security groups
3. Enable encryption at rest
4. Create database
5. Configure connection string

### Google Cloud SQL

```json
{
  "JobOptions": {
    "ConnectionString": "Host=/cloudsql/project:region:instance;Database=craftjobs;Username=postgres;Password=password"
  }
}
```

**Setup:**
1. Create Cloud SQL PostgreSQL instance
2. Configure IAM permissions
3. Enable SSL
4. Create database
5. Use Cloud SQL Proxy or configure connection string

---

## Connection String Parameters

### Common Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `Host` | Server address | `localhost` or `server.example.com` |
| `Port` | PostgreSQL port | `5432` (default) |
| `Database` | Database name | `CraftJobs` |
| `Username` | Database user | `postgres` or `jobuser` |
| `Password` | User password | `secure_password` |

### Security Parameters

| Parameter | Description | Values |
|-----------|-------------|--------|
| `SslMode` | SSL/TLS mode | `Disable`, `Require`, `Prefer` |
| `TrustServerCertificate` | Trust server cert | `true`, `false` |
| `SslCert` | Client certificate path | `/path/to/cert.crt` |

### Performance Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `Pooling` | Connection pooling | `true` |
| `MinPoolSize` | Min pool size | `1` |
| `MaxPoolSize` | Max pool size | `100` |
| `ConnectionLifetime` | Max connection age | `300` seconds |
| `Timeout` | Connection timeout | `15` seconds |
| `CommandTimeout` | Command timeout | `30` seconds |

### Example with All Parameters

```json
{
  "JobOptions": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CraftJobs;Username=jobuser;Password=password;Pooling=true;MinPoolSize=5;MaxPoolSize=100;ConnectionLifetime=300;Timeout=30;CommandTimeout=60;SslMode=Require"
  }
}
```

---

## Backup and Restore

### Backup

```bash
# Full database backup
pg_dump -h localhost -U postgres -d CraftJobs > craftjobs_backup.sql

# Schema-only backup
pg_dump -h localhost -U postgres -d CraftJobs --schema=hangfire > hangfire_schema.sql

# Data-only backup
pg_dump -h localhost -U postgres -d CraftJobs --data-only --schema=hangfire > hangfire_data.sql

# Compressed backup
pg_dump -h localhost -U postgres -d CraftJobs | gzip > craftjobs_backup.sql.gz
```

### Restore

```bash
# Full restore
psql -h localhost -U postgres -d CraftJobs < craftjobs_backup.sql

# Compressed restore
gunzip -c craftjobs_backup.sql.gz | psql -h localhost -U postgres -d CraftJobs
```

### Automated Backups

**Using cron (Linux):**
```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM
0 2 * * * pg_dump -h localhost -U postgres -d CraftJobs | gzip > /backups/craftjobs_$(date +\%Y\%m\%d).sql.gz
```

**Using Windows Task Scheduler:**
```powershell
# Create backup script (backup-jobs.ps1)
$date = Get-Date -Format "yyyyMMdd"
pg_dump -h localhost -U postgres -d CraftJobs | gzip > "C:\Backups\craftjobs_$date.sql.gz"

# Schedule task
schtasks /create /tn "JobDB Backup" /tr "powershell.exe -File C:\Scripts\backup-jobs.ps1" /sc daily /st 02:00
```

---

## Maintenance

### Clean Up Old Jobs

Hangfire automatically cleans up based on `JobExpirationDays`:

```json
{
  "JobOptions": {
    "JobExpirationDays": 7  // Delete jobs older than 7 days
  }
}
```

Manual cleanup:

```sql
-- Delete succeeded jobs older than 7 days
DELETE FROM hangfire.job 
WHERE statename = 'Succeeded' 
AND createdat < NOW() - INTERVAL '7 days';

-- Delete failed jobs older than 30 days
DELETE FROM hangfire.job 
WHERE statename = 'Failed' 
AND createdat < NOW() - INTERVAL '30 days';
```

### Vacuum and Analyze

```sql
-- Vacuum tables
VACUUM ANALYZE hangfire.job;
VACUUM ANALYZE hangfire.state;
VACUUM ANALYZE hangfire.jobqueue;

-- Or vacuum entire schema
VACUUM ANALYZE;
```

### Reindex

```sql
-- Reindex schema
REINDEX SCHEMA hangfire;

-- Or specific tables
REINDEX TABLE hangfire.job;
REINDEX TABLE hangfire.state;
```

---

## Monitoring

### Database Size

```sql
-- Total database size
SELECT pg_size_pretty(pg_database_size('CraftJobs'));

-- Schema size
SELECT pg_size_pretty(pg_total_relation_size('hangfire.job'));

-- All tables size
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'hangfire'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

### Active Connections

```sql
-- Current connections
SELECT 
    count(*) as connections,
    state
FROM pg_stat_activity
WHERE datname = 'CraftJobs'
GROUP BY state;

-- Detailed connection info
SELECT 
    pid,
    usename,
    application_name,
    state,
    query_start,
    state_change
FROM pg_stat_activity
WHERE datname = 'CraftJobs';
```

### Table Statistics

```sql
-- Row counts
SELECT 
    schemaname,
    tablename,
    n_live_tup as rows
FROM pg_stat_user_tables
WHERE schemaname = 'hangfire'
ORDER BY n_live_tup DESC;

-- Index usage
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan,
    idx_tup_read,
    idx_tup_fetch
FROM pg_stat_user_indexes
WHERE schemaname = 'hangfire'
ORDER BY idx_scan DESC;
```

---

## Troubleshooting

### Issue: "permission denied to create extension"

**Solution:**
```sql
-- Connect as superuser
\c CraftJobs postgres

-- Grant permissions
GRANT CREATE ON SCHEMA public TO jobuser;
```

### Issue: "schema does not exist"

**Cause:** Schema not created yet  
**Solution:** Run application once to auto-create schema

### Issue: "connection refused"

**Causes:**
- PostgreSQL not running
- Firewall blocking port 5432
- Wrong host/port in connection string

**Solutions:**
```bash
# Check PostgreSQL status
sudo systemctl status postgresql

# Check port listening
netstat -an | grep 5432

# Test connection
psql -h localhost -U postgres -d CraftJobs
```

### Issue: "too many connections"

**Solution:**
```sql
-- Check max connections
SHOW max_connections;

-- Increase max connections
ALTER SYSTEM SET max_connections = 200;

-- Restart PostgreSQL
sudo systemctl restart postgresql
```

### Issue: "database is locked"

**Solution:**
```sql
-- Check locks
SELECT * FROM pg_locks WHERE NOT granted;

-- Kill blocking query
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE state = 'active' AND pid != pg_backend_pid();
```

---

## Performance Tuning

### PostgreSQL Configuration

Edit `postgresql.conf`:

```conf
# Memory
shared_buffers = 256MB
effective_cache_size = 1GB
work_mem = 16MB
maintenance_work_mem = 64MB

# WAL
wal_buffers = 16MB
checkpoint_completion_target = 0.9

# Query Planner
random_page_cost = 1.1
effective_io_concurrency = 200

# Background Writer
bgwriter_lru_maxpages = 100
bgwriter_lru_multiplier = 2.0
```

### Connection Pooling

```json
{
  "JobOptions": {
    "ConnectionString": "Host=localhost;Database=CraftJobs;Pooling=true;MinPoolSize=10;MaxPoolSize=100"
  }
}
```

### Indexes

Hangfire creates all necessary indexes automatically.

---

## Security Best Practices

1. ? **Use dedicated user** - Don't use `postgres` superuser
2. ? **Enable SSL/TLS** - Always in production
3. ? **Use secrets management** - Azure Key Vault, AWS Secrets Manager
4. ? **Restrict network access** - Firewall rules, VPN
5. ? **Regular backups** - Automated daily backups
6. ? **Monitor access logs** - Track database access
7. ? **Rotate passwords** - Regular password changes
8. ? **Encrypt at rest** - Database encryption
9. ? **Audit permissions** - Regular permission reviews
10. ? **Use connection pooling** - Prevent connection exhaustion

---

## Summary

### ? Automatic Setup
- No manual migrations required
- Hangfire creates all tables automatically
- Just create database and configure connection string

### ? Production Ready
- Separate database recommended
- Dedicated user with minimal permissions
- SSL/TLS encryption
- Connection pooling
- Automated backups

### ? Scalable
- Multiple server support
- Optimized indexes
- Connection pooling
- Performance tuning

### ? Maintained
- Automatic cleanup
- Regular vacuuming
- Monitoring tools
- Backup/restore procedures

---

**Database setup is simple and automatic!** Just create the database, configure the connection string, and run your application. Hangfire handles the rest.

Happy job processing! ??
