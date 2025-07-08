using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResumeMatcher.Data;
using ResumeMatcher.Models;

namespace ResumeMatcher.Pages
{
    public class JobListingsModel : PageModel
    {
        private readonly ResumeMatcherDbContext _context;

        public JobListingsModel(ResumeMatcherDbContext context)
        {
            _context = context;
        }

        public List<JobPost> Jobs { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync(int page = 1)
        {
            int pageSize = 5;

            var query = _context.JobPosts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(j => j.JobTitle.Contains(SearchTerm) || j.Description.Contains(SearchTerm));
            }

            int totalJobs = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalJobs / (double)pageSize);
            CurrentPage = page;

            Jobs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
