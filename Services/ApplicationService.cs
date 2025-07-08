using Microsoft.EntityFrameworkCore;
using ResumeMatcher.Data;
using ResumeMatcher.Models;
using System.Text.Json;

namespace ResumeMatcher.Services
{
    public class ApplicationService
    {
        private readonly ResumeMatcherDbContext _context;
        private readonly ResumeMatchingService _matchingService;
        private readonly AnalyticsService _analyticsService;

        public ApplicationService(
            ResumeMatcherDbContext context,
            ResumeMatchingService matchingService,
            AnalyticsService analyticsService)
        {
            _context = context;
            _matchingService = matchingService;
            _analyticsService = analyticsService;
        }

        public async Task<JobApplication> CreateApplicationAsync(int jobPostId, int cvId, int? userId = null)
        {
            // Check if application already exists
            var existingApplication = await _context.JobApplications
                .FirstOrDefaultAsync(ja => ja.JobPostId == jobPostId && ja.CVId == cvId);

            if (existingApplication != null)
            {
                throw new InvalidOperationException("Application already exists for this job and resume.");
            }

            // Get job and CV data
            var jobPost = await _context.JobPosts.FindAsync(jobPostId);
            var cv = await _context.CVs.FindAsync(cvId);

            if (jobPost == null || cv == null)
            {
                throw new ArgumentException("Invalid job or CV ID.");
            }

            // Calculate match score
            var matchScore = _matchingService.CalculateMatchScore(cv.RawText ?? "", jobPost.Description ?? "");

            var application = new JobApplication
            {
                JobPostId = jobPostId,
                CVId = cvId,
                UserId = userId,
                MatchScore = matchScore,
                Status = ApplicationStatus.Applied
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            // Track analytics
            await _analyticsService.TrackMetricAsync("application", "created", 1, jobPostId, userId);
            await _analyticsService.TrackMetricAsync("match_score", "application_match", matchScore, jobPostId, userId);

            return application;
        }

        public async Task<JobApplication> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus newStatus, string? notes = null)
        {
            var application = await _context.JobApplications
                .Include(ja => ja.JobPost)
                .Include(ja => ja.CV)
                .FirstOrDefaultAsync(ja => ja.Id == applicationId);

            if (application == null)
            {
                throw new ArgumentException("Application not found.");
            }

            var oldStatus = application.Status;
            application.Status = newStatus;
            application.ReviewedAt = DateTime.Now;

            if (!string.IsNullOrEmpty(notes))
            {
                application.ReviewNotes = notes;
            }

            await _context.SaveChangesAsync();

            // Track status change
            await _analyticsService.TrackMetricAsync("application_status", newStatus.ToString(), 1,
                application.JobPostId, application.UserId,
                JsonSerializer.Serialize(new { OldStatus = oldStatus.ToString(), NewStatus = newStatus.ToString() }));

            return application;
        }

        public async Task<List<JobApplication>> GetApplicationsForJobAsync(int jobPostId, ApplicationStatus? status = null)
        {
            var query = _context.JobApplications
                .Include(ja => ja.CV)
                .Include(ja => ja.User)
                .Where(ja => ja.JobPostId == jobPostId);

            if (status.HasValue)
            {
                query = query.Where(ja => ja.Status == status.Value);
            }

            return await query
                .OrderByDescending(ja => ja.MatchScore)
                .ThenByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        public async Task<List<JobApplication>> GetApplicationsForUserAsync(int userId, ApplicationStatus? status = null)
        {
            var query = _context.JobApplications
                .Include(ja => ja.JobPost)
                .Include(ja => ja.CV)
                .Where(ja => ja.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(ja => ja.Status == status.Value);
            }

            return await query
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        public async Task<List<JobApplication>> GetShortlistedApplicationsAsync(int jobPostId)
        {
            return await _context.JobApplications
                .Include(ja => ja.CV)
                .Include(ja => ja.User)
                .Where(ja => ja.JobPostId == jobPostId && ja.IsShortlisted)
                .OrderByDescending(ja => ja.MatchScore)
                .ToListAsync();
        }

        public async Task ShortlistApplicationAsync(int applicationId, bool isShortlisted = true)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);
            if (application == null)
            {
                throw new ArgumentException("Application not found.");
            }

            application.IsShortlisted = isShortlisted;
            if (isShortlisted && application.Status == ApplicationStatus.Applied)
            {
                application.Status = ApplicationStatus.Shortlisted;
            }

            await _context.SaveChangesAsync();

            // Track shortlisting
            await _analyticsService.TrackMetricAsync("application_action",
                isShortlisted ? "shortlisted" : "unshortlisted", 1,
                application.JobPostId, application.UserId);
        }

        public async Task ScheduleInterviewAsync(int applicationId, DateTime interviewDate, string? notes = null)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);
            if (application == null)
            {
                throw new ArgumentException("Application not found.");
            }

            application.InterviewScheduledAt = interviewDate;
            application.InterviewNotes = notes;
            application.Status = ApplicationStatus.InterviewScheduled;

            await _context.SaveChangesAsync();

            // Track interview scheduling
            await _analyticsService.TrackMetricAsync("interview", "scheduled", 1,
                application.JobPostId, application.UserId);
        }

        public async Task<ApplicationStats> GetApplicationStatsAsync(int jobPostId)
        {
            var applications = await _context.JobApplications
                .Where(ja => ja.JobPostId == jobPostId)
                .ToListAsync();

            return new ApplicationStats
            {
                TotalApplications = applications.Count,
                AverageMatchScore = applications.Any() ? applications.Average(a => a.MatchScore) : 0,
                ShortlistedCount = applications.Count(a => a.IsShortlisted),
                InterviewsScheduled = applications.Count(a => a.Status == ApplicationStatus.InterviewScheduled),
                StatusBreakdown = applications
                    .GroupBy(a => a.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }

        public async Task<List<JobApplication>> GetTopMatchesForJobAsync(int jobPostId, int count = 10)
        {
            return await _context.JobApplications
                .Include(ja => ja.CV)
                .Include(ja => ja.User)
                .Where(ja => ja.JobPostId == jobPostId)
                .OrderByDescending(ja => ja.MatchScore)
                .Take(count)
                .ToListAsync();
        }

        public async Task AddHRFeedbackAsync(int applicationId, string feedback)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);
            if (application == null)
            {
                throw new ArgumentException("Application not found.");
            }

            application.HRFeedback = feedback;
            application.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }

    public class ApplicationStats
    {
        public int TotalApplications { get; set; }
        public double AverageMatchScore { get; set; }
        public int ShortlistedCount { get; set; }
        public int InterviewsScheduled { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; } = new();
    }
}
