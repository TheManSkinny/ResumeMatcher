using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResumeMatcher.Data;
using ResumeMatcher.Models;
using ResumeMatcher.Services;

namespace ResumeMatcher.Pages
{
    public class ApplicationsModel : PageModel
    {
        private readonly ResumeMatcherDbContext _context;
        private readonly ApplicationService _applicationService;

        public ApplicationsModel(ResumeMatcherDbContext context, ApplicationService applicationService)
        {
            _context = context;
            _applicationService = applicationService;
        }

        public List<JobApplication> Applications { get; set; } = new();
        public List<JobPost> Jobs { get; set; } = new();

        public string? SelectedJobId { get; set; }
        public string? SelectedStatus { get; set; }
        public string? SelectedMatchRange { get; set; }

        public async Task OnGetAsync(int? jobId, string? status, string? matchRange)
        {
            SelectedJobId = jobId?.ToString();
            SelectedStatus = status;
            SelectedMatchRange = matchRange;

            // Get all jobs for the filter dropdown
            Jobs = await _context.JobPosts
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            // Build the query
            var query = _context.JobApplications
                .Include(ja => ja.JobPost)
                .Include(ja => ja.CV)
                .Include(ja => ja.User)
                .AsQueryable();

            // Apply filters
            if (jobId.HasValue)
            {
                query = query.Where(ja => ja.JobPostId == jobId.Value);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var statusEnum))
            {
                query = query.Where(ja => ja.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(matchRange))
            {
                query = matchRange switch
                {
                    "excellent" => query.Where(ja => ja.MatchScore >= 70),
                    "good" => query.Where(ja => ja.MatchScore >= 40 && ja.MatchScore < 70),
                    "fair" => query.Where(ja => ja.MatchScore < 40),
                    _ => query
                };
            }

            Applications = await query
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }
    }
}
