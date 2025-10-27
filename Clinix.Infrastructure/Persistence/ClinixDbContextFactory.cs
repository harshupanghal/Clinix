using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clinix.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Prevents background services from starting during migration operations.
/// </summary>
public class ClinixDbContextFactory : IDesignTimeDbContextFactory<ClinixDbContext>
    {
    public ClinixDbContext CreateDbContext(string[] args)
        {
        // Use your actual connection string here
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=ClxDb;Trusted_Connection=True;MultipleActiveResultSets=true;";
        
        var optionsBuilder = new DbContextOptionsBuilder<ClinixDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ClinixDbContext(optionsBuilder.Options);
        }
    }
