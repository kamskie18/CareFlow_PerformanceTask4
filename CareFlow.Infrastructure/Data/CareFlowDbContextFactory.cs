using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CareFlow.Infrastructure.Data;

public class CareFlowDbContextFactory : IDesignTimeDbContextFactory<CareFlowDbContext>
{
    public CareFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CareFlowDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=careflow_db;Username=postgres;Password=password");

        return new CareFlowDbContext(optionsBuilder.Options);
    }
}
