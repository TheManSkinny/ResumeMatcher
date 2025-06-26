using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ResumeMatcher.Pages
{
    public class UploadCVModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public UploadCVModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [BindProperty]
        public IFormFile CVFile { get; set; }

        public string UploadMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (CVFile == null || CVFile.Length == 0)
            {
                UploadMessage = "Please select a valid file.";
                return Page();
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, CVFile.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await CVFile.CopyToAsync(stream);
            }

            UploadMessage = $"CV uploaded successfully: {CVFile.FileName}";
            return Page();
        }
    }
}
