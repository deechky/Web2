using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TripService.Data
{
    public class TripDbContextFactory : IDesignTimeDbContextFactory<TripDbContext>
    {
        public TripDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TripDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("TripsDB"));

            return new TripDbContext(optionsBuilder.Options);
        }
    }
}
