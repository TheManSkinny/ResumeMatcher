using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ResumeMatcher.Data;
using ResumeMatcher.Models;

namespace ResumeMatcher.Pages
{
    public class AddJobModel : PageModel
    {
        private readonly ResumeMatcherDbContext _context;

        public AddJobModel(ResumeMatcherDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? JobTitle { get; set; }

        [BindProperty]
        public string? Description { get; set; }

        public string? Message { get; set; }

        public void OnGet()
        {
            // Initialize page
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(JobTitle) || string.IsNullOrWhiteSpace(Description))
            {
                Message = "All fields are required.";
                return Page();
            }

            var job = new JobPost
            {
                JobTitle = JobTitle,
                Description = Description
            };

            _context.JobPosts.Add(job);
            await _context.SaveChangesAsync();

            Message = "Job post added successfully!";
            JobTitle = string.Empty;
            Description = string.Empty;
            
            return Page();
        }
    }
}
