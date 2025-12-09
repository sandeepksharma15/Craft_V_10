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
        public string? ReceivedBuildInput { get; private set; }
        public string? ReceivedValidateInput { get; private set; }
        public string? ReceivedMaskInput { get; private set; }

        public string Build(string connectionString)
        {
            BuildCallCount++;
            ReceivedBuildInput = connectionString;
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
        public string Build(string connectionString) => "BETA_BUILD_RESULT";
        public bool Validate(string connectionString) => false; // Always invalid
        public string Mask(string connectionString) => "BETA_MASKED";
    }
    #endregion

    private static ConnectionStringService CreateService(params IConnectionStringHandler[] handlers) => new(handlers);

    [Fact]
    public void Build_DelegatesToCorrectHandler_ReturnsExpectedResult()
    {
        // Arrange
        var alpha = new AlphaConnectionStringHandler();
        var beta = new BetaConnectionStringHandler();
        var sut = CreateService(alpha, beta);
        var connectionString = "Server=(local);Database=Db;Trusted_Connection=True;";

        // Act
        var result = sut.Build(connectionString, "Alpha");

        // Assert
        Assert.Equal("ALPHA_BUILD_RESULT", result);
        Assert.Equal(1, alpha.BuildCallCount);
        Assert.Equal(connectionString, alpha.ReceivedBuildInput);
    }

    [Fact]
    public void Validate_DelegatesPassingConnectionString_ReturnsHandlerResult()
    {
        // Arrange
        var alpha = new AlphaConnectionStringHandler();
        var sut = CreateService(alpha);
        var cs = "Server=(local);Database=Db;Trusted_Connection=True;";

        // Act
        var result = sut.Validate(cs, "Alpha");

        // Assert
        Assert.True(result);
        Assert.Equal(1, alpha.ValidateCallCount);
        Assert.Equal(cs, alpha.ReceivedValidateInput);
    }

    [Fact]
    public void Mask_DelegatesPassingConnectionString_ReturnsHandlerResult()
    {
        // Arrange
        var alpha = new AlphaConnectionStringHandler();
        var sut = CreateService(alpha);
        var cs = "Server=(local);Database=Db;User Id=sa;Password=Secret;";

        // Act
        var result = sut.Mask(cs, "Alpha");

        // Assert
        Assert.Equal("ALPHA_MASKED", result);
        Assert.Equal(1, alpha.MaskCallCount);
        Assert.Equal(cs, alpha.ReceivedMaskInput);
    }

    [Fact]
    public void ProviderLookup_IsCaseInsensitive()
    {
        // Arrange - provider specified in different casing than handler key
        var alpha = new AlphaConnectionStringHandler();
        var sut = CreateService(alpha);
        var cs = "Server=(local);Database=Db;Trusted_Connection=True;";

        // Act
        var buildResult = sut.Build(cs, "alpha"); // lower-case

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
        var sut = CreateService(alpha, beta);
        var cs = "Server=(local);Database=Db;Trusted_Connection=True;";

        // Act
        var buildResult = sut.Build(cs, "Beta");
        var validateResult = sut.Validate(cs, "Beta");
        var maskResult = sut.Mask(cs, "Beta");

        // Assert
        Assert.Equal("BETA_BUILD_RESULT", buildResult);
        Assert.False(validateResult);
        Assert.Equal("BETA_MASKED", maskResult);
        Assert.Equal(0, alpha.BuildCallCount);
        Assert.Equal(0, alpha.ValidateCallCount);
        Assert.Equal(0, alpha.MaskCallCount);
    }

    [Fact]
    public void UnknownProvider_ThrowsNotSupportedException()
    {
        // Arrange
        var alpha = new AlphaConnectionStringHandler();
        var sut = CreateService(alpha);
        var cs = "Server=(local);Database=Db;Trusted_Connection=True;";

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => sut.Build(cs, "Gamma"));
        Assert.Contains("Gamma", ex.Message);
    }
}
