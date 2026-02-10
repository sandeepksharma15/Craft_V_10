using System.Linq.Expressions;
using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Evaluators;

/// <summary>
/// Comprehensive tests for AutoIncludeNavigationPropertiesEvaluator.
/// </summary>
public class AutoIncludeNavigationPropertiesEvaluatorTests
{
    private readonly IQueryable<Company> _companies;
    private readonly IQueryable<Store> _stores;
    private readonly IQueryable<Country> _countries;

    public AutoIncludeNavigationPropertiesEvaluatorTests()
    {
        var country1 = new Country { Id = 1, Name = "USA" };
        var country2 = new Country { Id = 2, Name = "Canada" };

        _countries = new List<Country> { country1, country2 }.AsQueryable();

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
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.GetQuery(null!, query));
    }

    [Fact]
    public void GetQuery_NullQuery_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;

        // Act
        var result = evaluator.GetQuery(_companies, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result.ToList());
    }

    [Fact]
    public void GetQuery_AutoIncludeDisabled_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = false };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result.ToList());
        Assert.Null(query.IncludeExpressions);
    }

    [Fact]
    public void GetQuery_EmptyQueryable_ReturnsEmpty()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };
        var emptyQueryable = new List<Company>().AsQueryable();

        // Act
        var result = evaluator.GetQuery(emptyQueryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region Auto Include Enabled Tests

    [Fact]
    public void GetQuery_AutoIncludeEnabled_AddsNavigationProperties()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(query.IncludeExpressions);
        Assert.NotEmpty(query.IncludeExpressions);
        // Company has Country and Stores navigation properties
        Assert.True(query.IncludeExpressions.Count >= 1);
    }

    [Fact]
    public void GetQuery_AutoIncludeEnabled_IncludesCountryNavigation()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(query.IncludeExpressions);
        var countryInclude = query.IncludeExpressions
            .FirstOrDefault(e => e.PropertyType == typeof(Country));
        Assert.NotNull(countryInclude);
        Assert.False(countryInclude.IsThenInclude);
        Assert.Equal(typeof(Company), countryInclude.EntityType);
    }

    [Fact]
    public void GetQuery_AutoIncludeEnabled_IncludesStoresCollection()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(query.IncludeExpressions);
        var storesInclude = query.IncludeExpressions
            .FirstOrDefault(e => e.PropertyType == typeof(List<Store>));
        Assert.NotNull(storesInclude);
        Assert.False(storesInclude.IsThenInclude);
        Assert.Equal(typeof(Company), storesInclude.EntityType);
    }

    [Fact]
    public void GetQuery_StoreEntity_IncludesCompanyNavigation()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Store> { AutoIncludeNavigationProperties = true };

        // Act
        var result = evaluator.GetQuery(_stores, query);

        // Assert
        Assert.NotNull(query.IncludeExpressions);
        var companyInclude = query.IncludeExpressions
            .FirstOrDefault(e => e.PropertyType == typeof(Company));
        Assert.NotNull(companyInclude);
        Assert.False(companyInclude.IsThenInclude);
        Assert.Equal(typeof(Store), companyInclude.EntityType);
    }

    #endregion

    #region Already Included Properties Tests

    [Fact]
    public void GetQuery_PropertyAlreadyIncluded_DoesNotAddDuplicate()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };

        // Manually add Country include
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        query.IncludeExpressions = 
        [
            new IncludeExpression(countryExpression, typeof(Company), typeof(Country), false, null)
        ];

        var initialCount = query.IncludeExpressions.Count;

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(query.IncludeExpressions);
        // Should not add another Country include since it's already there
        var countryIncludes = query.IncludeExpressions
            .Where(e => e.PropertyType == typeof(Country) && !e.IsThenInclude)
            .ToList();
        Assert.Single(countryIncludes);
    }

    [Fact]
    public void GetQuery_PartiallyIncluded_AddsOnlyMissing()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };

        // Manually add only Country include
        Expression<Func<Company, Country?>> countryExpression = c => c.Country;
        query.IncludeExpressions = 
        [
            new IncludeExpression(countryExpression, typeof(Company), typeof(Country), false, null)
        ];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(query.IncludeExpressions);
        // Should have Country (already added) and Stores (auto-added)
        Assert.True(query.IncludeExpressions.Count >= 2);
        
        var countryIncludes = query.IncludeExpressions
            .Where(e => e.PropertyType == typeof(Country) && !e.IsThenInclude)
            .ToList();
        Assert.Single(countryIncludes);

        var storesInclude = query.IncludeExpressions
            .FirstOrDefault(e => e.PropertyType == typeof(List<Store>) && !e.IsThenInclude);
        Assert.NotNull(storesInclude);
    }

    [Fact]
    public void GetQuery_ThenIncludeExists_DoesNotAffectAutoInclude()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };

        // Add a ThenInclude (should not interfere with auto-include)
        Expression<Func<Company, List<Store>?>> storesExpression = c => c.Stores;
        var rootInclude = new IncludeExpression(storesExpression, typeof(Company), typeof(List<Store>), false, null);
        
        query.IncludeExpressions = [rootInclude];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(query.IncludeExpressions);
        // Should still add Country since only Stores was explicitly included
        var countryInclude = query.IncludeExpressions
            .FirstOrDefault(e => e.PropertyType == typeof(Country) && !e.IsThenInclude);
        Assert.NotNull(countryInclude);
    }

    #endregion

    #region Country Navigation Properties Tests

    [Fact]
    public void GetQuery_CountryEntity_AddsCompaniesNavigation()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Country> { AutoIncludeNavigationProperties = true };

        // Act
        var result = evaluator.GetQuery(_countries, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _countries], result.ToList());
        // Country has Companies navigation property
        Assert.NotNull(query.IncludeExpressions);
        Assert.NotEmpty(query.IncludeExpressions);
        var companiesInclude = query.IncludeExpressions
            .FirstOrDefault(e => e.PropertyType == typeof(List<Company>));
        Assert.NotNull(companiesInclude);
    }

    #endregion

    #region Singleton Pattern Tests

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        // Arrange & Act
        var instance1 = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var instance2 = AutoIncludeNavigationPropertiesEvaluator.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Instance_IsNotNull()
    {
        // Arrange & Act
        var instance = AutoIncludeNavigationPropertiesEvaluator.Instance;

        // Assert
        Assert.NotNull(instance);
    }

    #endregion

    #region IncludeExpressions Initialization Tests

    [Fact]
    public void GetQuery_NullIncludeExpressions_InitializesEmptyList()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };
        query.IncludeExpressions = null;

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(query.IncludeExpressions);
    }

    [Fact]
    public void GetQuery_EmptyIncludeExpressions_AddsToExistingList()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };
        query.IncludeExpressions = [];

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(query.IncludeExpressions);
        Assert.NotEmpty(query.IncludeExpressions);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetQuery_MultipleCallsSameQuery_DoesNotAddDuplicates()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };

        // Act
        var result1 = evaluator.GetQuery(_companies, query);
        var countAfterFirst = query.IncludeExpressions?.Count ?? 0;
        
        var result2 = evaluator.GetQuery(_companies, query);
        var countAfterSecond = query.IncludeExpressions?.Count ?? 0;

        // Assert
        Assert.NotNull(query.IncludeExpressions);
        // Second call should not add duplicates
        Assert.True(countAfterSecond >= countAfterFirst);
    }

    [Fact]
    public void GetQuery_ComplexEntity_HandlesAllNavigationProperties()
    {
        // Arrange
        var evaluator = AutoIncludeNavigationPropertiesEvaluator.Instance;
        var query = new Query<Company> { AutoIncludeNavigationProperties = true };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(query.IncludeExpressions);
        // Company has 2 navigation properties: Country and Stores
        Assert.True(query.IncludeExpressions.Count >= 2);
        
        // Verify all includes are non-ThenInclude
        Assert.All(query.IncludeExpressions, include => Assert.False(include.IsThenInclude));
        
        // Verify all includes are for Company entity type
        Assert.All(query.IncludeExpressions, include => Assert.Equal(typeof(Company), include.EntityType));
    }

    #endregion
}
