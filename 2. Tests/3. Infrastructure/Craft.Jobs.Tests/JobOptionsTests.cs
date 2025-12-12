namespace Craft.Jobs.Tests;

public class JobOptionsTests
{
    [Fact]
    public void JobOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new JobOptions();

        // Assert
        Assert.Equal(string.Empty, options.ConnectionString);
        Assert.True(options.EnableDashboard);
        Assert.Equal("/hangfire", options.DashboardPath);
        Assert.Equal(20, options.WorkerCount);
        Assert.Equal(3, options.MaxRetryAttempts);
        Assert.True(options.EnableAutomaticRetry);
        Assert.Equal(7, options.JobExpirationDays);
        Assert.False(options.EnableMultiTenancy);
        Assert.Equal("hangfire", options.SchemaName);
        Assert.Equal(15, options.PollingIntervalSeconds);
        Assert.False(options.EnableDetailedLogging);
    }

    [Fact]
    public void Validate_WithEmptyConnectionString_ReturnsError()
    {
        // Arrange
        var options = new JobOptions
        {
            ConnectionString = string.Empty
        };
        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(JobOptions.ConnectionString)));
    }

    [Fact]
    public void Validate_WithInvalidConnectionString_ReturnsError()
    {
        // Arrange
        var options = new JobOptions
        {
            ConnectionString = "InvalidConnectionString"
        };
        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("PostgreSQL"));
    }

    [Fact]
    public void Validate_WithValidConnectionString_ReturnsNoErrors()
    {
        // Arrange
        var options = new JobOptions
        {
            ConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test"
        };
        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WithEmptyDashboardPath_ReturnsError()
    {
        // Arrange
        var options = new JobOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            DashboardPath = string.Empty
        };
        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(JobOptions.DashboardPath)));
    }

    [Fact]
    public void Validate_WithDashboardPathNotStartingWithSlash_ReturnsError()
    {
        // Arrange
        var options = new JobOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            DashboardPath = "hangfire"
        };
        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Contains(results, r => r.ErrorMessage!.Contains("must start with '/'"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void WorkerCount_WithValidValue_PassesValidation(int workerCount)
    {
        // Arrange
        var options = new JobOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            WorkerCount = workerCount
        };
        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void MaxRetryAttempts_WithValidValue_PassesValidation(int maxRetryAttempts)
    {
        // Arrange
        var options = new JobOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            MaxRetryAttempts = maxRetryAttempts
        };
        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(365)]
    public void JobExpirationDays_WithValidValue_PassesValidation(int expirationDays)
    {
        // Arrange
        var options = new JobOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            JobExpirationDays = expirationDays
        };
        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void CanSetAndGet_AllProperties()
    {
        // Arrange & Act
        var options = new JobOptions
        {
            ConnectionString = "Host=test;Database=testdb",
            EnableDashboard = false,
            DashboardPath = "/custom",
            WorkerCount = 50,
            MaxRetryAttempts = 5,
            EnableAutomaticRetry = false,
            JobExpirationDays = 30,
            EnableMultiTenancy = true,
            SchemaName = "custom_schema",
            PollingIntervalSeconds = 30,
            EnableDetailedLogging = true
        };

        // Assert
        Assert.Equal("Host=test;Database=testdb", options.ConnectionString);
        Assert.False(options.EnableDashboard);
        Assert.Equal("/custom", options.DashboardPath);
        Assert.Equal(50, options.WorkerCount);
        Assert.Equal(5, options.MaxRetryAttempts);
        Assert.False(options.EnableAutomaticRetry);
        Assert.Equal(30, options.JobExpirationDays);
        Assert.True(options.EnableMultiTenancy);
        Assert.Equal("custom_schema", options.SchemaName);
        Assert.Equal(30, options.PollingIntervalSeconds);
        Assert.True(options.EnableDetailedLogging);
    }
}
