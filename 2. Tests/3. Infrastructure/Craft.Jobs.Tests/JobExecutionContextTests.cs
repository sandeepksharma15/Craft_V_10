namespace Craft.Jobs.Tests;

public class JobExecutionContextTests
{
    [Fact]
    public void JobExecutionContext_DefaultConstructor_InitializesDefaults()
    {
        // Arrange & Act
        var context = new JobExecutionContext();

        // Assert
        Assert.Equal(string.Empty, context.JobId);
        Assert.Equal(string.Empty, context.JobType);
        Assert.Equal(0, context.RetryAttempt);
        Assert.Null(context.TenantId);
        Assert.Null(context.UserId);
        Assert.NotEqual(default, context.CreatedAt);
        Assert.Null(context.StartedAt);
        Assert.NotNull(context.Metadata);
        Assert.Empty(context.Metadata);
    }

    [Fact]
    public void JobExecutionContext_CanSetAllProperties()
    {
        // Arrange
        var jobId = Guid.NewGuid().ToString();
        var tenantId = "tenant-123";
        var userId = "user-456";
        var createdAt = DateTimeOffset.UtcNow;
        var startedAt = DateTimeOffset.UtcNow.AddSeconds(5);
        var metadata = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        var context = new JobExecutionContext
        {
            JobId = jobId,
            JobType = "TestJob",
            RetryAttempt = 2,
            TenantId = tenantId,
            UserId = userId,
            CreatedAt = createdAt,
            StartedAt = startedAt,
            Metadata = metadata
        };

        // Assert
        Assert.Equal(jobId, context.JobId);
        Assert.Equal("TestJob", context.JobType);
        Assert.Equal(2, context.RetryAttempt);
        Assert.Equal(tenantId, context.TenantId);
        Assert.Equal(userId, context.UserId);
        Assert.Equal(createdAt, context.CreatedAt);
        Assert.Equal(startedAt, context.StartedAt);
        Assert.Equal(2, context.Metadata.Count);
        Assert.Equal("value1", context.Metadata["key1"]);
        Assert.Equal("value2", context.Metadata["key2"]);
    }

    [Fact]
    public void JobExecutionContext_Metadata_CanAddItems()
    {
        // Arrange
        var context = new JobExecutionContext();

        // Act
        context.Metadata["test"] = "value";
        context.Metadata["another"] = "data";

        // Assert
        Assert.Equal(2, context.Metadata.Count);
        Assert.Equal("value", context.Metadata["test"]);
        Assert.Equal("data", context.Metadata["another"]);
    }

    [Fact]
    public void JobExecutionContext_CreatedAt_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var context = new JobExecutionContext();

        // Assert
        var after = DateTimeOffset.UtcNow;
        Assert.True(context.CreatedAt >= before);
        Assert.True(context.CreatedAt <= after);
    }
}
