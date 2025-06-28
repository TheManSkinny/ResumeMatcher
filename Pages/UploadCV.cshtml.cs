using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResumeMatcher.Data;
using ResumeMatcher.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ResumeMatcher.Pages
{
    public class UploadCVModel : PageModel
    {
        private readonly ResumeMatcherDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UploadCVModel(ResumeMatcherDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public string? FullName { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? PhoneNumber { get; set; }

        [BindProperty]
        public IFormFile? CVFile { get; set; }

        public string? Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || CVFile == null)
            {
                Message = "Please fill all fields and upload a CV.";
                return Page();
            }

            // Prevent duplicate uploads
            var exists = await _context.CVs.AnyAsync(c => c.FileName == CVFile.FileName && c.Email == Email);
            if (exists)
            {
                Message = "This resume has already been uploaded.";
                return Page();
            }

            // Save file to disk
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, CVFile.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await CVFile.CopyToAsync(stream);
            }

            // Extract text from PDF
            string extractedText = "[text extraction failed]";
            try
            {
                using (var pdf = PdfDocument.Open(filePath))
                {
                    extractedText = string.Join("\n", pdf.GetPages().Select(p => p.Text));
                }
            }
            catch (Exception ex)
            {
                extractedText = $"[text extraction failed: {ex.Message}]";
            }

            // Create CV object
            var cv = new CV
            {
                FullName = FullName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                FilePath = $"/uploads/{CVFile.FileName}",
                FileName = CVFile.FileName,
                UploadedAt = DateTime.Now,
                RawText = extractedText
            };

            // Find best job match
            var jobPosts = await _context.JobPosts.ToListAsync();
            string bestMatch = "No jobs available";
            int maxScore = 0;

            foreach (var job in jobPosts)
            {
                var score = GetMatchScore(extractedText, job.Description);
                if (score > maxScore)
                {
                    maxScore = score;
                    bestMatch = job.JobTitle;
                }
            }

            // Save CV to database (only once)
            _context.CVs.Add(cv);
            await _context.SaveChangesAsync();

            // Set success message with match result (only once)
            Message = maxScore > 0 
                ? $"Resume uploaded successfully! Best match: {bestMatch} ({maxScore} keyword matches)"
                : "Resume uploaded successfully! No job matches found.";

            return Page();
        }

        private int GetMatchScore(string resumeText, string jobDescription)
        {
            var resumeWords = resumeText.ToLower().Split(' ', '\n', '.', ',', ';');
            var jobWords = jobDescription.ToLower().Split(' ', '\n', '.', ',', ';');

            return resumeWords.Intersect(jobWords).Count();
        }
    }
}
