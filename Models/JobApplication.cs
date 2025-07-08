using System.ComponentModel.DataAnnotations;

namespace ResumeMatcher.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        [Required]
        public int JobPostId { get; set; }
        public JobPost JobPost { get; set; } = null!;

        [Required]
        public int CVId { get; set; }
        public CV CV { get; set; } = null!;

        public int? UserId { get; set; }
        public User? User { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

        public float MatchScore { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewNotes { get; set; }

        public string? HRFeedback { get; set; }

        // Interview scheduling
        public DateTime? InterviewScheduledAt { get; set; }
        public string? InterviewNotes { get; set; }

        public bool IsShortlisted { get; set; }
    }

    public enum ApplicationStatus
    {
        Applied = 1,
        Reviewing = 2,
        Shortlisted = 3,
        InterviewScheduled = 4,
        Interviewed = 5,
        Offered = 6,
        Rejected = 7,
        Withdrawn = 8,
        Hired = 9
    }
}
