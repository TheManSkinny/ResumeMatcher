using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ResumeMatcher.Data;
using ResumeMatcher.Models;
using System.Threading.Tasks;

namespace ResumeMatcher.Pages
{
    public class EditJobModel : PageModel
    {
        private readonly ResumeMatcherDbContext _context;

        public EditJobModel(ResumeMatcherDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public JobPost JobPost { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            JobPost = await _context.JobPosts.FindAsync(id);
            if (JobPost == null)
                return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var jobInDb = await _context.JobPosts.FindAsync(JobPost.Id);
            if (jobInDb == null) return NotFound();

            jobInDb.JobTitle = JobPost.JobTitle;
            jobInDb.Description = JobPost.Description;
            await _context.SaveChangesAsync();

            return RedirectToPage("/JobList");
        }
    }
}
