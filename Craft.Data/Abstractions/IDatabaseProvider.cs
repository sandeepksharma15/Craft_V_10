using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

public interface IDatabaseProvider
{
    void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString);
    bool ValidateConnection(string connectionString);
}
