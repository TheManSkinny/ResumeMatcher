
using Microsoft.EntityFrameworkCore;
using ResumeMatcher.Models;

namespace ResumeMatcher.Data
{
    public class ResumeMatcherDbContext : DbContext
    {
        public ResumeMatcherDbContext(DbContextOptions<ResumeMatcherDbContext> options)
            : base(options)
        {
        }

        public DbSet<CV> CVs { get; set; } 
        public DbSet<JobPost> JobPosts { get; set; }

    }
}
