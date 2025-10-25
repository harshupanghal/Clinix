using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clinix.Infrastructure.Persistence;

public class ClinixDbContextFactory : IDesignTimeDbContextFactory<ClinixDbContext>
    {
    public ClinixDbContext CreateDbContext(string[] args)
        {
        var optionsBuilder = new DbContextOptionsBuilder<ClinixDbContext>();

        // Use same default connection you use in dev. Change if you want another during migrations.
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=ClxDb;Trusted_Connection=True;";
        optionsBuilder.UseSqlServer(connectionString);

        return new ClinixDbContext(optionsBuilder.Options);
        }
    }
