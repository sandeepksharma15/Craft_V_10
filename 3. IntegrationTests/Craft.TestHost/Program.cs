using Craft.Core;
using Craft.MultiTenant;
using Craft.MultiTenant.Stores;
using Craft.Repositories;
using Craft.TestHost.Data;
using Craft.TestHost.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<TestHostDbContext>(options =>
    options.UseInMemoryDatabase("TestHostDb"));

builder.Services.AddScoped<IDbContext>(provider =>
    provider.GetRequiredService<TestHostDbContext>());

// Add Repository
builder.Services.AddScoped<IChangeRepository<TestProduct>, ChangeRepository<TestProduct>>();

// Add Multi-tenant
builder.Services.AddMultiTenant<Tenant>()
    .WithHeaderStrategy(TenantConstants.TenantToken)
    .WithInMemoryStore(options =>
    {
        options.Tenants =
        [
            new Tenant(1, "Alpha Corp", string.Empty, "alpha") { IsActive = true },
            new Tenant(2, "Beta Inc", string.Empty, "beta") { IsActive = true }
        ];
    });

var app = builder.Build();

// Ensure database is created with seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TestHostDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseMultiTenant();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Required for WebApplicationFactory
#pragma warning disable ASP0027 // Unnecessary public Program class declaration
public partial class Program { }
#pragma warning restore ASP0027 // Unnecessary public Program class declaration

