using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResumeMatcher.Data;
using ResumeMatcher.Models;
using ResumeMatcher.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace ResumeMatcher.Pages
{
    public class UploadCVModel : PageModel
    {
        private readonly ResumeMatcherDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ResumeMatchingService _matchingService;
        private readonly DocumentProcessingService _documentService;

        public UploadCVModel(
            ResumeMatcherDbContext context, 
            IWebHostEnvironment environment, 
            ResumeMatchingService matchingService,
            DocumentProcessingService documentService)
        {
            _context = context;
            _environment = environment;
            _matchingService = matchingService;
            _documentService = documentService;
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
        public bool IsError { get; set; }
        public List<(string JobTitle, float MatchScore)> JobMatches { get; set; } = new();
        public ResumeAnalysis? ResumeAnalysis { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || CVFile == null)
            {
                Message = "Please fill all fields and upload a CV.";
                IsError = true;
                return Page();
            }

            // File validation
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(CVFile.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                Message = "Please upload a PDF, DOC, or DOCX file.";
                IsError = true;
                return Page();
            }

            if (CVFile.Length > 5 * 1024 * 1024) // 5MB limit
            {
                Message = "File size must be less than 5MB.";
                IsError = true;
                return Page();
            }

            // Check for valid email format
            if (!IsValidEmail(Email))
            {
                Message = "Please enter a valid email address.";
                IsError = true;
                return Page();
            }

            // Check for valid email format
            if (!IsValidEmail(Email))
            {
                Message = "Please enter a valid email address.";
                IsError = true;
                return Page();
            }

            // Prevent duplicate uploads
            var exists = await _context.CVs.AnyAsync(c => c.FileName == CVFile.FileName && c.Email == Email);
            if (exists)
            {
                Message = "This resume has already been uploaded.";
                IsError = true;
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

            // Extract text from uploaded file
            string extractedText;
            try
            {
                extractedText = await _documentService.ExtractTextFromFileAsync(filePath);
                ResumeAnalysis = _documentService.AnalyzeResume(extractedText);
            }
            catch (Exception ex)
            {
                Message = $"Error processing resume: {ex.Message}";
                IsError = true;
                return Page();
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

            // Find best job matches using ML.NET service
            var jobPosts = await _context.JobPosts.ToListAsync();
            var jobsForMatching = jobPosts
                .Where(j => j.JobTitle != null && j.Description != null)
                .Select(j => (j.JobTitle!, j.Description!))
                .ToList();

            if (jobsForMatching.Any())
            {
                JobMatches = _matchingService.FindBestMatches(extractedText, jobsForMatching, 5);
                var bestMatch = JobMatches.FirstOrDefault();

                Message = bestMatch.MatchScore > 0
                    ? $"Resume uploaded successfully! Best match: {bestMatch.JobTitle} ({bestMatch.MatchScore:F1}% match)"
                    : "Resume uploaded successfully! No strong job matches found.";
            }
            else
            {
                Message = "Resume uploaded successfully! No jobs available for matching.";
            }

            // Save CV to database
            _context.CVs.Add(cv);
            await _context.SaveChangesAsync();

            return Page();
        }

        private bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
