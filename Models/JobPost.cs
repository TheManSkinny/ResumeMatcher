using System.ComponentModel.DataAnnotations;

namespace ResumeMatcher.Models
{
    public class JobPost
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [Required]
        [Display(Name = "Job Description")]
        public string? Description { get; set; }

        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Salary Range")]
        public string? SalaryRange { get; set; }

        [Display(Name = "Experience Level")]
        public string? ExperienceLevel { get; set; }

        [Display(Name = "Job Type")]
        public string? JobType { get; set; }

        [Display(Name = "Required Skills")]
        public string? RequiredSkills { get; set; }

        [Display(Name = "Company")]
        public string? Company { get; set; }

        [Display(Name = "Posted Date")]
        public DateTime PostedAt { get; set; } = DateTime.Now;

        [Display(Name = "Application Deadline")]
        public DateTime? ApplicationDeadline { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // HR/Company user who posted the job
        public string? PostedBy { get; set; }
    }
}
