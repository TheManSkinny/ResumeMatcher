using System;
using System.ComponentModel.DataAnnotations;



namespace ResumeMatcher.Models
{
    public class CV
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        [Display(Name = "Resume File Path")]
        public string? FilePath { get; set; }

        public string? FileName { get; set; }

        public string? RawText { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Enhanced fields
        public string? ExtractedSkills { get; set; } // JSON array of skills
        public int YearsOfExperience { get; set; }
        public string? Education { get; set; } // JSON array of education
        public string? Summary { get; set; }
        public double QualityScore { get; set; } // Overall resume quality (0-100)

        // User relationship
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Navigation properties
        public List<JobApplication> Applications { get; set; } = new();
    }
}
// This file defines the CV model used in the ResumeMatcher application.
// It includes properties for the CV's ID, full name, email, phone number, file