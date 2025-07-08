using Microsoft.EntityFrameworkCore;
using ResumeMatcher.Data;
using ResumeMatcher.Models;
using System.Text.Json;

namespace ResumeMatcher.Services
{
    public class AnalyticsService
    {
        private readonly ResumeMatcherDbContext _context;

        public AnalyticsService(ResumeMatcherDbContext context)
        {
            _context = context;
        }

        public async Task<MatchMetrics> GetMatchMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddDays(-30);
            endDate ??= DateTime.Now;

            var jobs = await _context.JobPosts
                .Where(j => j.PostedAt >= startDate && j.PostedAt <= endDate)
                .ToListAsync();

            var resumes = await _context.CVs
                .Where(cv => cv.UploadedAt >= startDate && cv.UploadedAt <= endDate)
                .ToListAsync();

            var applications = await _context.JobApplications
                .Include(ja => ja.JobPost)
                .Include(ja => ja.CV)
                .Where(ja => ja.AppliedAt >= startDate && ja.AppliedAt <= endDate)
                .ToListAsync();

            var averageMatchScore = applications.Any() ? applications.Average(a => a.MatchScore) : 0;
            var successfulMatches = applications.Count(a => a.MatchScore > 50);

            return new MatchMetrics
            {
                TotalJobs = jobs.Count,
                TotalResumes = resumes.Count,
                TotalApplications = applications.Count,
                AverageMatchScore = averageMatchScore,
                SuccessfulMatches = successfulMatches,
                TopSkills = await GetTopSkillsAsync(startDate.Value, endDate.Value),
                SkillDemands = await GetSkillDemandsAsync(),
                ApplicationTrends = await GetApplicationTrendsAsync(startDate.Value, endDate.Value)
            };
        }

        public async Task<List<string>> GetTopSkillsAsync(DateTime startDate, DateTime endDate)
        {
            var jobs = await _context.JobPosts
                .Where(j => j.PostedAt >= startDate && j.PostedAt <= endDate && !string.IsNullOrEmpty(j.RequiredSkills))
                .Select(j => j.RequiredSkills)
                .ToListAsync();

            var skillCounts = new Dictionary<string, int>();

            foreach (var jobSkills in jobs)
            {
                if (string.IsNullOrEmpty(jobSkills)) continue;

                var skills = jobSkills.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));

                foreach (var skill in skills)
                {
                    skillCounts[skill] = skillCounts.GetValueOrDefault(skill, 0) + 1;
                }
            }

            return skillCounts
                .OrderByDescending(kv => kv.Value)
                .Take(10)
                .Select(kv => kv.Key)
                .ToList();
        }

        public async Task<List<SkillDemand>> GetSkillDemandsAsync()
        {
            var jobSkills = await _context.JobPosts
                .Where(j => !string.IsNullOrEmpty(j.RequiredSkills))
                .Select(j => j.RequiredSkills)
                .ToListAsync();

            var resumeSkills = await _context.CVs
                .Where(cv => !string.IsNullOrEmpty(cv.ExtractedSkills))
                .Select(cv => cv.ExtractedSkills)
                .ToListAsync();

            var jobSkillCounts = CountSkills(jobSkills);
            var resumeSkillCounts = CountSkills(resumeSkills);

            var skillDemands = new List<SkillDemand>();

            foreach (var jobSkill in jobSkillCounts)
            {
                var resumeCount = resumeSkillCounts.GetValueOrDefault(jobSkill.Key, 0);
                var demandRatio = resumeCount > 0 ? (double)jobSkill.Value / resumeCount : jobSkill.Value;

                skillDemands.Add(new SkillDemand
                {
                    Skill = jobSkill.Key,
                    JobCount = jobSkill.Value,
                    ResumeCount = resumeCount,
                    DemandRatio = demandRatio
                });
            }

            return skillDemands
                .OrderByDescending(sd => sd.DemandRatio)
                .Take(20)
                .ToList();
        }

        public async Task<List<ApplicationTrend>> GetApplicationTrendsAsync(DateTime startDate, DateTime endDate)
        {
            var applications = await _context.JobApplications
                .Where(ja => ja.AppliedAt >= startDate && ja.AppliedAt <= endDate)
                .GroupBy(ja => ja.AppliedAt.Date)
                .Select(g => new ApplicationTrend
                {
                    Date = g.Key,
                    ApplicationCount = g.Count(),
                    AverageMatchScore = g.Average(ja => ja.MatchScore)
                })
                .OrderBy(at => at.Date)
                .ToListAsync();

            var jobCounts = await _context.JobPosts
                .Where(j => j.PostedAt >= startDate && j.PostedAt <= endDate)
                .GroupBy(j => j.PostedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Merge job counts with application trends
            foreach (var trend in applications)
            {
                var jobCount = jobCounts.FirstOrDefault(jc => jc.Date == trend.Date);
                trend.JobCount = jobCount?.Count ?? 0;
            }

            return applications;
        }

        public async Task TrackMetricAsync(string metricType, string metricName, double value, int? jobPostId = null, int? userId = null, string? dimensions = null)
        {
            var analyticsData = new AnalyticsData
            {
                MetricType = metricType,
                MetricName = metricName,
                Value = value,
                JobPostId = jobPostId,
                UserId = userId,
                Dimensions = dimensions
            };

            _context.Analytics.Add(analyticsData);
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<string, object>> GetDashboardDataAsync()
        {
            var today = DateTime.Today;
            var lastWeek = today.AddDays(-7);
            var lastMonth = today.AddDays(-30);

            var totalJobs = await _context.JobPosts.CountAsync();
            var totalResumes = await _context.CVs.CountAsync();
            var totalApplications = await _context.JobApplications.CountAsync();

            var jobsThisWeek = await _context.JobPosts.CountAsync(j => j.PostedAt >= lastWeek);
            var resumesThisWeek = await _context.CVs.CountAsync(cv => cv.UploadedAt >= lastWeek);
            var applicationsThisWeek = await _context.JobApplications.CountAsync(ja => ja.AppliedAt >= lastWeek);

            var topMatchingJobs = await _context.JobApplications
                .Include(ja => ja.JobPost)
                .GroupBy(ja => ja.JobPost)
                .Select(g => new { Job = g.Key, AverageMatch = g.Average(ja => ja.MatchScore), ApplicationCount = g.Count() })
                .OrderByDescending(x => x.AverageMatch)
                .Take(5)
                .ToListAsync();

            return new Dictionary<string, object>
            {
                ["TotalJobs"] = totalJobs,
                ["TotalResumes"] = totalResumes,
                ["TotalApplications"] = totalApplications,
                ["JobsThisWeek"] = jobsThisWeek,
                ["ResumesThisWeek"] = resumesThisWeek,
                ["ApplicationsThisWeek"] = applicationsThisWeek,
                ["TopMatchingJobs"] = topMatchingJobs
            };
        }

        private Dictionary<string, int> CountSkills(List<string?> skillLists)
        {
            var skillCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var skillList in skillLists)
            {
                if (string.IsNullOrEmpty(skillList)) continue;

                List<string> skills;
                try
                {
                    // Try to parse as JSON array first
                    skills = JsonSerializer.Deserialize<List<string>>(skillList) ?? new List<string>();
                }
                catch
                {
                    // Fall back to comma-separated parsing
                    skills = skillList.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }

                foreach (var skill in skills)
                {
                    skillCounts[skill] = skillCounts.GetValueOrDefault(skill, 0) + 1;
                }
            }

            return skillCounts;
        }
    }
}
