using System.ComponentModel.DataAnnotations;

namespace ResumeMatcher.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public string? Company { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public List<JobApplication> Applications { get; set; } = new();
        public List<JobPost> PostedJobs { get; set; } = new();
    }

    public enum UserRole
    {
        JobSeeker = 1,
        HRManager = 2,
        Admin = 3
    }
}
