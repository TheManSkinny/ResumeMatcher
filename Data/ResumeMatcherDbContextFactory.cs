using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ResumeMatcher.Data
{
    public class ResumeMatcherDbContextFactory : IDesignTimeDbContextFactory<ResumeMatcherDbContext>
    {
        public ResumeMatcherDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ResumeMatcherDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new ResumeMatcherDbContext(optionsBuilder.Options);
        }
    }
}
