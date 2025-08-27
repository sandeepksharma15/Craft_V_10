using Microsoft.Extensions.Options;
using Xunit;

namespace Craft.Data.Tests.ConnectionStringHandlers;

/// <summary>
/// Unit tests for <see cref="ConnectionStringService"/> ensuring full coverage of delegation logic
/// and edge cases (provider resolution, case-insensitivity, unsupported provider path).
/// </summary>
public class ConnectionStringServiceTests
{
    #region Test Stub Handlers
    private sealed class AlphaConnectionStringHandler : IConnectionStringHandler
    {
        public int BuildCallCount { get; private set; }
        public int ValidateCallCount { get; private set; }
        public int MaskCallCount { get; private set; }
        public DatabaseOptions? ReceivedBuildOptions { get; private set; }
        public string? ReceivedValidateInput { get; private set; }
        public string? ReceivedMaskInput { get; private set; }

        public string Build(DatabaseOptions options)
        {
            BuildCallCount++;
            ReceivedBuildOptions = options;
            return "ALPHA_BUILD_RESULT";
        }

        public bool Validate(string connectionString)
        {
            ValidateCallCount++;
            ReceivedValidateInput = connectionString;
            return true; // Always valid for test purposes
        }

        public string Mask(string connectionString)
        {
            MaskCallCount++;
            ReceivedMaskInput = connectionString;
            return "ALPHA_MASKED";
        }
    }

    private sealed class BetaConnectionStringHandler : IConnectionStringHandler
    {
        public string Build(DatabaseOptions options) => "BETA_BUILD_RESULT";
        public bool Validate(string connectionString) => false; // Always invalid
        public string Mask(string connectionString) => "BETA_MASKED";
    }
    #endregion

    private static ConnectionStringService CreateService(DatabaseOptions options, params IConnectionStringHandler[] handlers) =>
        new(handlers, Options.Create(options));

    [Fact]
    public void Build_DelegatesToCorrectHandler_ReturnsExpectedResult()
    {
        // Arrange
        var alpha = new AlphaConnectionStringHandler();
        var beta = new BetaConnectionStringHandler();
        var options = new DatabaseOptions
        {
            DbProvider = "Alpha", // Matches AlphaConnectionStringHandler (suffix stripped)
            ConnectionString = "Server=(local);Database=Db;Trusted_Connection=True;"
        };
        var sut = CreateService(options, alpha, beta);

        // Act
        var result = sut.Build();

        // Assert
        Assert.Equal("ALPHA_BUILD_RESULT", result);
        Assert.Equal(1, alpha.BuildCallCount);
        Assert.Same(options, alpha.ReceivedBuildOptions);
    }

    [Fact]
    public void Validate_DelegatesPassingConnectionString_ReturnsHandlerResult()
    {
        // Arrange
        var alpha = new AlphaConnectionStringHandler();
        var options = new DatabaseOptions
        {
            DbProvider = "Alpha",
            ConnectionString = "Server=(local);Database=Db;Trusted_Connection=True;"
        };
        var sut = CreateService(options, alpha);

        // Act
        var result = sut.Validate();

        // Assert
        Assert.True(result); // Alpha returns true
        Assert.Equal(1, alpha.ValidateCallCount);
        Assert.Equal(options.ConnectionString, alpha.ReceivedValidateInput);
    }

    [Fact]
    public void Mask_DelegatesPassingConnectionString_ReturnsHandlerResult()
    {
        // Arrange
        var alpha = new AlphaConnectionStringHandler();
        var options = new DatabaseOptions
        {
            DbProvider = "Alpha",
            ConnectionString = "Server=(local);Database=Db;User Id=sa;Password=Secret;"
        };
        var sut = CreateService(options, alpha);

        // Act
        var result = sut.Mask();

        // Assert
        Assert.Equal("ALPHA_MASKED", result);
        Assert.Equal(1, alpha.MaskCallCount);
        Assert.Equal(options.ConnectionString, alpha.ReceivedMaskInput);
    }

    [Fact]
    public void ProviderLookup_IsCaseInsensitive()
    {
        // Arrange - provider specified in different casing than handler key
        var alpha = new AlphaConnectionStringHandler();
        var options = new DatabaseOptions
        {
            DbProvider = "alpha", // lower-case
            ConnectionString = "Server=(local);Database=Db;Trusted_Connection=True;"
        };
        var sut = CreateService(options, alpha);

        // Act
        var buildResult = sut.Build();

        // Assert
        Assert.Equal("ALPHA_BUILD_RESULT", buildResult);
        Assert.Equal(1, alpha.BuildCallCount);
    }

    [Fact]
    public void MultipleHandlers_PicksHandlerMatchingProvider()
    {
        // Arrange - ensure Beta chosen (Alpha present but provider = Beta)
        var alpha = new AlphaConnectionStringHandler();
        var beta = new BetaConnectionStringHandler();
        var options = new DatabaseOptions
        {
            DbProvider = "Beta",
            ConnectionString = "Server=(local);Database=Db;Trusted_Connection=True;"
        };
        var sut = CreateService(options, alpha, beta);

        // Act
        var buildResult = sut.Build();
        var validateResult = sut.Validate();
        var maskResult = sut.Mask();

        // Assert
        Assert.Equal("BETA_BUILD_RESULT", buildResult);
        Assert.False(validateResult); // Beta returns false
        Assert.Equal("BETA_MASKED", maskResult);
        Assert.Equal(0, alpha.BuildCallCount); // Alpha should be untouched
        Assert.Equal(0, alpha.ValidateCallCount);
        Assert.Equal(0, alpha.MaskCallCount);
    }

    [Fact]
    public void UnknownProvider_ThrowsNotSupportedException()
    {
        // Arrange
        var alpha = new AlphaConnectionStringHandler();
        var options = new DatabaseOptions
        {
            DbProvider = "Gamma", // No matching handler
            ConnectionString = "Server=(local);Database=Db;Trusted_Connection=True;"
        };
        var sut = CreateService(options, alpha);

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => sut.Build());
        Assert.Contains("Gamma", ex.Message);
    }
}
