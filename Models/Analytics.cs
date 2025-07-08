using System.ComponentModel.DataAnnotations;

namespace ResumeMatcher.Models
{
    public class AnalyticsData
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string MetricType { get; set; } = string.Empty; // e.g., "job_views", "applications", "matches"

        public string MetricName { get; set; } = string.Empty;

        public double Value { get; set; }

        public string? Dimensions { get; set; } // JSON string for additional dimensions

        public int? JobPostId { get; set; }
        public JobPost? JobPost { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }
    }

    public class MatchMetrics
    {
        public int TotalJobs { get; set; }
        public int TotalResumes { get; set; }
        public int TotalApplications { get; set; }
        public double AverageMatchScore { get; set; }
        public int SuccessfulMatches { get; set; } // Matches that led to applications
        public List<string> TopSkills { get; set; } = new();
        public List<SkillDemand> SkillDemands { get; set; } = new();
        public List<ApplicationTrend> ApplicationTrends { get; set; } = new();
    }

    public class SkillDemand
    {
        public string Skill { get; set; } = string.Empty;
        public int JobCount { get; set; }
        public int ResumeCount { get; set; }
        public double DemandRatio { get; set; } // JobCount / ResumeCount
    }

    public class ApplicationTrend
    {
        public DateTime Date { get; set; }
        public int ApplicationCount { get; set; }
        public int JobCount { get; set; }
        public double AverageMatchScore { get; set; }
    }
}
