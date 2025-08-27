using Craft.Core;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

public interface IDbContextFactory<T> where T : DbContext, IDbContext
{
    T CreateDbContext();
}
