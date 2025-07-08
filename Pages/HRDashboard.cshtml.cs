using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResumeMatcher.Data;
using ResumeMatcher.Models;
using ResumeMatcher.Services;

namespace ResumeMatcher.Pages
{
    public class HRDashboardModel : PageModel
    {
        private readonly ResumeMatcherDbContext _context;
        private readonly AnalyticsService _analyticsService;

        public HRDashboardModel(ResumeMatcherDbContext context, AnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        public Dictionary<string, object> DashboardData { get; set; } = new();
        public List<JobApplication> RecentApplications { get; set; } = new();
        public List<string> TopSkills { get; set; } = new();
        public List<ApplicationTrend> ApplicationTrends { get; set; } = new();
        public List<int> MatchQualityDistribution { get; set; } = new() { 0, 0, 0 };

        public async Task OnGetAsync()
        {
            // Get dashboard data
            DashboardData = await _analyticsService.GetDashboardDataAsync();

            // Get recent applications
            RecentApplications = await _context.JobApplications
                .Include(ja => ja.JobPost)
                .Include(ja => ja.CV)
                .Include(ja => ja.User)
                .OrderByDescending(ja => ja.AppliedAt)
                .Take(20)
                .ToListAsync();

            // Get top skills
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);
            TopSkills = await _analyticsService.GetTopSkillsAsync(startDate, endDate);

            // Get application trends
            ApplicationTrends = await _analyticsService.GetApplicationTrendsAsync(startDate, endDate);

            // Calculate match quality distribution
            var allApplications = await _context.JobApplications.ToListAsync();
            if (allApplications.Any())
            {
                var excellent = allApplications.Count(a => a.MatchScore >= 70);
                var good = allApplications.Count(a => a.MatchScore >= 40 && a.MatchScore < 70);
                var fair = allApplications.Count(a => a.MatchScore < 40);

                MatchQualityDistribution = new List<int> { excellent, good, fair };
            }
        }
    }
}
