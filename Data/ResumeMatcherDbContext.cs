
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
        public DbSet<User> Users { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<AnalyticsData> Analytics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.JobPost)
                .WithMany()
                .HasForeignKey(ja => ja.JobPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.CV)
                .WithMany(cv => cv.Applications)
                .HasForeignKey(ja => ja.CVId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(ja => ja.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CV>()
                .HasOne(cv => cv.User)
                .WithMany()
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes for better performance
            modelBuilder.Entity<JobApplication>()
                .HasIndex(ja => ja.Status);

            modelBuilder.Entity<JobApplication>()
                .HasIndex(ja => ja.AppliedAt);

            modelBuilder.Entity<JobPost>()
                .HasIndex(jp => jp.PostedAt);

            modelBuilder.Entity<JobPost>()
                .HasIndex(jp => jp.IsActive);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

    }
}
