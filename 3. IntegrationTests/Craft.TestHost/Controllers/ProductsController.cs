using Craft.Controllers;
using Craft.Repositories;
using Craft.TestHost.Entities;
using Microsoft.Extensions.Logging;

namespace Craft.TestHost.Controllers;

/// <summary>
/// Product controller for HTTP integration tests.
/// </summary>
public class ProductsController : EntityChangeController<TestProduct, TestProductDto, KeyType>
{
    public ProductsController(
        IChangeRepository<TestProduct> repository,
        ILogger<ProductsController> logger)
        : base(repository, logger)
    {
    }
}
