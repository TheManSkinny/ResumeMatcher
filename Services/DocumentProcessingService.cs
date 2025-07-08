using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using System.Text.RegularExpressions;

namespace ResumeMatcher.Services
{
    public class DocumentProcessingService
    {
        public async Task<string> ExtractTextFromFileAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            try
            {
                return extension switch
                {
                    ".pdf" => ExtractTextFromPdf(filePath),
                    ".docx" => await ExtractTextFromDocxAsync(filePath),
                    ".doc" => ExtractTextFromDoc(filePath),
                    ".txt" => await File.ReadAllTextAsync(filePath),
                    _ => throw new NotSupportedException($"File type {extension} is not supported")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error extracting text from file: {ex.Message}", ex);
            }
        }

        private string ExtractTextFromPdf(string filePath)
        {
            using var pdf = PdfDocument.Open(filePath);
            var text = new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }

        private async Task<string> ExtractTextFromDocxAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using var doc = WordprocessingDocument.Open(filePath, false);
                var body = doc.MainDocumentPart?.Document?.Body;

                if (body == null) return string.Empty;

                var text = new StringBuilder();
                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    text.AppendLine(paragraph.InnerText);
                }

                return text.ToString();
            });
        }

        private string ExtractTextFromDoc(string filePath)
        {
            // Note: For .doc files, you might need Microsoft.Office.Interop.Word
            // For now, return a message indicating limited support
            throw new NotSupportedException("Legacy .doc files require additional setup. Please use .docx format.");
        }

        public ResumeAnalysis AnalyzeResume(string resumeText)
        {
            return new ResumeAnalysis
            {
                ExtractedText = resumeText,
                Skills = ExtractSkills(resumeText),
                YearsOfExperience = ExtractYearsOfExperience(resumeText),
                Education = ExtractEducation(resumeText),
                Email = ExtractEmail(resumeText),
                PhoneNumber = ExtractPhoneNumber(resumeText),
                Summary = GenerateSummary(resumeText)
            };
        }

        private List<string> ExtractSkills(string text)
        {
            var technicalSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Programming Languages
                "JavaScript", "TypeScript", "C#", "Java", "Python", "C++", "C", "PHP", "Ruby", "Go", "Rust",
                "Swift", "Kotlin", "Scala", "R", "MATLAB", "Perl", "Shell", "Bash", "PowerShell",
                
                // Web Technologies
                "HTML", "CSS", "React", "Angular", "Vue.js", "Node.js", "Express", "jQuery", "Bootstrap",
                "SASS", "LESS", "Webpack", "Gulp", "Grunt", "NPM", "Yarn",
                
                // Databases
                "SQL", "MySQL", "PostgreSQL", "MongoDB", "SQLite", "Oracle", "SQL Server", "Redis",
                "Elasticsearch", "DynamoDB", "Firebase", "Cassandra",
                
                // Cloud & DevOps
                "AWS", "Azure", "Google Cloud", "Docker", "Kubernetes", "Jenkins", "Git", "GitHub",
                "GitLab", "Bitbucket", "CI/CD", "Terraform", "Ansible", "Chef", "Puppet",
                
                // Frameworks
                ".NET", "ASP.NET", "Entity Framework", "Spring", "Django", "Flask", "Laravel",
                "Rails", "Express.js", "Symfony",
                
                // Tools
                "Visual Studio", "VS Code", "IntelliJ", "Eclipse", "Xcode", "Postman", "JIRA"
            };

            var foundSkills = new List<string>();
            var lowerText = text.ToLower();

            foreach (var skill in technicalSkills)
            {
                if (lowerText.Contains(skill.ToLower()))
                {
                    foundSkills.Add(skill);
                }
            }

            return foundSkills.Distinct().ToList();
        }

        private int ExtractYearsOfExperience(string text)
        {
            var patterns = new[]
            {
                @"(\d+)\+?\s*years?\s+(?:of\s+)?experience",
                @"(\d+)\s*-\s*\d+\s*years?\s+experience",
                @"(\d+)\s*years?\s+in",
                @"(\d+)\s*years?\s+with"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int years))
                {
                    return years;
                }
            }

            return 0;
        }

        private List<string> ExtractEducation(string text)
        {
            var educationKeywords = new[]
            {
                "Bachelor", "Master", "PhD", "Doctorate", "Associate", "Diploma",
                "B.S.", "B.A.", "M.S.", "M.A.", "MBA", "Ph.D.",
                "Computer Science", "Engineering", "Mathematics", "Physics",
                "Business Administration", "Information Technology"
            };

            var education = new List<string>();
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (educationKeywords.Any(keyword =>
                    line.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                {
                    education.Add(line.Trim());
                }
            }

            return education.Take(3).ToList(); // Limit to top 3 education entries
        }

        private string? ExtractEmail(string text)
        {
            var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            var match = Regex.Match(text, emailPattern);
            return match.Success ? match.Value : null;
        }

        private string? ExtractPhoneNumber(string text)
        {
            var phonePatterns = new[]
            {
                @"\b\d{3}-\d{3}-\d{4}\b",
                @"\b\(\d{3}\)\s*\d{3}-\d{4}\b",
                @"\b\d{10}\b",
                @"\b\+1\s*\d{3}\s*\d{3}\s*\d{4}\b"
            };

            foreach (var pattern in phonePatterns)
            {
                var match = Regex.Match(text, pattern);
                if (match.Success)
                {
                    return match.Value;
                }
            }

            return null;
        }

        private string GenerateSummary(string text)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var wordCount = words.Length;

            return $"Resume contains {wordCount} words and appears to be a " +
                   (wordCount > 500 ? "comprehensive" : wordCount > 200 ? "standard" : "brief") +
                   " resume document.";
        }
    }

    public class ResumeAnalysis
    {
        public string ExtractedText { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public int YearsOfExperience { get; set; }
        public List<string> Education { get; set; } = new();
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
