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
    }
}
// This file defines the CV model used in the ResumeMatcher application.
// It includes properties for the CV's ID, full name, email, phone number, file