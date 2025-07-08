using Microsoft.AspNetCore.Mvc.RazorPages;
using ResumeMatcher.Data;
using ResumeMatcher.Models;
using Microsoft.EntityFrameworkCore;

namespace ResumeMatcher.Pages
{
    public class JobListModel : PageModel
    {
        private readonly ResumeMatcherDbContext _context;

        public JobListModel(ResumeMatcherDbContext context)
        {
            _context = context;
        }

        public List<JobPost> JobPosts { get; set; } = new();

        public async Task OnGetAsync()
        {
            JobPosts = await _context.JobPosts
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
        }
    }
}
