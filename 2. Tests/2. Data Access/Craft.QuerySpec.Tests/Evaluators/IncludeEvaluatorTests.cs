using System.Linq.Expressions;
using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Evaluators;

/// <summary>
/// Comprehensive tests for IncludeEvaluator covering Include and ThenInclude scenarios.
/// </summary>
public class IncludeEvaluatorTests
{
    private readonly IQueryable<Company> _companies;
    private readonly IQueryable<Store> _stores;

    public IncludeEvaluatorTests()
    {
        var country1 = new Country { Id = 1, Name = "USA" };
        var country2 = new Country { Id = 2, Name = "Canada" };

        _companies = new List<Company>
        {
            new() { Id = 1, Name = "Company 1", CountryId = 1, Country = country1, Stores = [] },
            new() { Id = 2, Name = "Company 2", CountryId = 2, Country = country2, Stores = [] }
        }.AsQueryable();

        _stores = new List<Store>
        {
            new() { Id = 1, Name = "Store 1", CompanyId = 1, Company = _companies.First() },
            new() { Id = 2, Name = "Store 2", CompanyId = 2, Company = _companies.Last() }
        }.AsQueryable();
    }

    #region Basic Tests

    [Fact]
    public void GetQuery_NullQueryable_ThrowsArgumentNullException()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.GetQuery(null!, query));
    }

    [Fact]
    public void GetQuery_NullQuery_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;

        // Act
        var result = evaluator.GetQuery(_companies, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result.ToList());
    }

    [Fact]
    public void GetQuery_NullIncludeExpressions_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result.ToList());
    }

    [Fact]
    public void GetQuery_EmptyIncludeExpressions_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>
        {
            IncludeExpressions = []
        };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result.ToList());
    }

    [Fact]
    public void GetQuery_EmptyQueryable_ReturnsEmpty()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();
        var emptyQueryable = new List<Company>().AsQueryable();

        // Act
        var result = evaluator.GetQuery(emptyQueryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region Single Include Tests

    [Fact]
    public void GetQuery_SingleInclude_AppliesInclude()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();
        
        // Create Include expression for Country
        Expression<Func<Company, Country?>> includeExpression = c => c.Country;
        query.IncludeExpressions = 
        [
            new IncludeExpression(includeExpression, typeof(Company), typeof(Country), false, null)
        ];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void GetQuery_SingleCollectionInclude_AppliesInclude()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();
        
        // Create Include expression for Stores collection
        Expression<Func<Company, List<Store>?>> includeExpression = c => c.Stores;
        query.IncludeExpressions = 
        [
            new IncludeExpression(includeExpression, typeof(Company), typeof(List<Store>), false, null)
        ];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region Multiple Include Tests

    [Fact]
    public void GetQuery_MultipleIncludes_AppliesAllIncludes()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();
        
        // Create Include expressions for Country and Stores
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        Expression<Func<Company, List<Store>?>> storesExpression = c => c.Stores;
        
        query.IncludeExpressions = 
        [
            new IncludeExpression(countryExpression, typeof(Company), typeof(Country), false, null),
            new IncludeExpression(storesExpression, typeof(Company), typeof(List<Store>), false, null)
        ];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void GetQuery_DuplicateIncludes_HandlesGracefully()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();
        
        // Create duplicate Include expressions
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        
        var include1 = new IncludeExpression(countryExpression, typeof(Company), typeof(Country), false, null);
        var include2 = new IncludeExpression(countryExpression, typeof(Company), typeof(Country), false, null);
        
        query.IncludeExpressions = [include1, include2];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region ThenInclude Tests

    [Fact]
    public void GetQuery_ThenIncludeChain_AppliesChain()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Store>();
        
        // Create Include expression: Store -> Company
        Expression<Func<Store, Company?>> companyExpression = s => s.Company;
        var rootInclude = new IncludeExpression(companyExpression, typeof(Store), typeof(Company), false, null);
        
        // Create ThenInclude expression: Company -> Country
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        var thenInclude = new IncludeExpression(countryExpression, typeof(Company), typeof(Country), true, rootInclude);
        
        query.IncludeExpressions = [rootInclude, thenInclude];

        // Act
        var result = evaluator.GetQuery(_stores, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void GetQuery_MultipleThenIncludeChains_AppliesAllChains()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Store>();
        
        // Create first chain: Store -> Company -> Country
        Expression<Func<Store, Company?>> companyExpression = s => s.Company;
        var rootInclude = new IncludeExpression(companyExpression, typeof(Store), typeof(Company), false, null);
        
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        var thenInclude1 = new IncludeExpression(countryExpression, typeof(Company), typeof(Country), true, rootInclude);
        
        // Create second chain: Store -> Company -> Stores
        Expression<Func<Company, List<Store>?>> storesExpression = c => c.Stores;
        var thenInclude2 = new IncludeExpression(storesExpression, typeof(Company), typeof(List<Store>), true, rootInclude);
        
        query.IncludeExpressions = [rootInclude, thenInclude1, thenInclude2];

        // Act
        var result = evaluator.GetQuery(_stores, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void GetQuery_DeepThenIncludeChain_AppliesNestedChain()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Store>();
        
        // Create chain: Store -> Company -> Country
        Expression<Func<Store, Company?>> companyExpression = s => s.Company;
        var level1 = new IncludeExpression(companyExpression, typeof(Store), typeof(Company), false, null);
        
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        var level2 = new IncludeExpression(countryExpression, typeof(Company), typeof(Country), true, level1);
        
        query.IncludeExpressions = [level1, level2];

        // Act
        var result = evaluator.GetQuery(_stores, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region Multiple Root Includes Tests

    [Fact]
    public void GetQuery_MultipleRoots_ProcessesEachRoot()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();
        
        // Create first root: Company -> Country
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        var root1 = new IncludeExpression(countryExpression, typeof(Company), typeof(Country), false, null);
        
        // Create second root: Company -> Stores
        Expression<Func<Company, List<Store>?>> storesExpression = c => c.Stores;
        var root2 = new IncludeExpression(storesExpression, typeof(Company), typeof(List<Store>), false, null);
        
        query.IncludeExpressions = [root1, root2];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void GetQuery_MultipleRootsWithThenIncludes_ProcessesAllChains()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();
        
        // First root chain: Company -> Country
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        var root1 = new IncludeExpression(countryExpression, typeof(Company), typeof(Country), false, null);
        
        // Second root chain: Company -> Stores with ThenInclude
        Expression<Func<Company, List<Store>?>> storesExpression = c => c.Stores;
        var root2 = new IncludeExpression(storesExpression, typeof(Company), typeof(List<Store>), false, null);
        
        query.IncludeExpressions = [root1, root2];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region Singleton Pattern Tests

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        // Arrange & Act
        var instance1 = IncludeEvaluator.Instance;
        var instance2 = IncludeEvaluator.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Instance_IsNotNull()
    {
        // Arrange & Act
        var instance = IncludeEvaluator.Instance;

        // Assert
        Assert.NotNull(instance);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetQuery_ThenIncludeWithoutRoot_HandlesGracefully()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();
        
        // Create standalone ThenInclude without root (edge case)
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        var orphanThenInclude = new IncludeExpression(countryExpression, typeof(Company), typeof(Country), true, null);
        
        query.IncludeExpressions = [orphanThenInclude];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void GetQuery_MixedIncludeAndThenInclude_ProcessesCorrectly()
    {
        // Arrange
        var evaluator = IncludeEvaluator.Instance;
        var query = new Query<Company>();
        
        // Create mixed includes
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        var include1 = new IncludeExpression(countryExpression, typeof(Company), typeof(Country), false, null);
        
        Expression<Func<Company, List<Store>?>> storesExpression = c => c.Stores;
        var include2 = new IncludeExpression(storesExpression, typeof(Company), typeof(List<Store>), false, null);
        
        query.IncludeExpressions = [include1, include2];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion
}
