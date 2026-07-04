using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SharingService.Data
{
    public class SharingDbContextFactory : IDesignTimeDbContextFactory<SharingDbContext>
    {
        public SharingDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<SharingDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("SharingDB"));

            return new SharingDbContext(optionsBuilder.Options);
        }
    }
}
