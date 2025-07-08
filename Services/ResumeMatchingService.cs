using Microsoft.ML;
using Microsoft.ML.Data;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ResumeMatcher.Services
{
    public class ResumeMatchingService
    {
        private readonly MLContext _mlContext;
        private readonly Dictionary<string, HashSet<string>> _skillSynonyms;
        private readonly HashSet<string> _technicalSkills;

        public ResumeMatchingService()
        {
            _mlContext = new MLContext(seed: 0);
            _skillSynonyms = InitializeSkillSynonyms();
            _technicalSkills = InitializeTechnicalSkills();
        }

        public class ResumeJobData
        {
            [LoadColumn(0)]
            public string ResumeText { get; set; } = string.Empty;

            [LoadColumn(1)]
            public string JobDescription { get; set; } = string.Empty;

            [LoadColumn(2)]
            public float MatchScore { get; set; }
        }

        public class ResumeJobPrediction
        {
            [ColumnName("Score")]
            public float MatchScore { get; set; }
        }

        public float CalculateMatchScore(string resumeText, string jobDescription)
        {
            // Enhanced matching algorithm
            var resumeSkills = ExtractSkills(resumeText);
            var jobSkills = ExtractSkills(jobDescription);
            var resumeKeywords = ExtractKeywords(resumeText);
            var jobKeywords = ExtractKeywords(jobDescription);

            if (!jobSkills.Any() && !jobKeywords.Any()) return 0;

            // Calculate skill match score (weighted higher)
            float skillMatchScore = CalculateSkillMatchScore(resumeSkills, jobSkills);
            
            // Calculate keyword match score
            float keywordMatchScore = CalculateKeywordMatchScore(resumeKeywords, jobKeywords);

            // Calculate experience level match
            float experienceMatchScore = CalculateExperienceMatch(resumeText, jobDescription);

            // Weighted combination: Skills 50%, Keywords 30%, Experience 20%
            return (skillMatchScore * 0.5f + keywordMatchScore * 0.3f + experienceMatchScore * 0.2f);
        }

        private float CalculateSkillMatchScore(HashSet<string> resumeSkills, HashSet<string> jobSkills)
        {
            if (!jobSkills.Any()) return 0;

            int directMatches = resumeSkills.Intersect(jobSkills, StringComparer.OrdinalIgnoreCase).Count();
            int synonymMatches = 0;

            // Check for synonym matches
            foreach (var jobSkill in jobSkills)
            {
                if (resumeSkills.Contains(jobSkill, StringComparer.OrdinalIgnoreCase)) continue;

                foreach (var (skill, synonyms) in _skillSynonyms)
                {
                    if (synonyms.Contains(jobSkill, StringComparer.OrdinalIgnoreCase) &&
                        resumeSkills.Any(rs => synonyms.Contains(rs, StringComparer.OrdinalIgnoreCase)))
                    {
                        synonymMatches++;
                        break;
                    }
                }
            }

            return ((float)(directMatches + synonymMatches) / jobSkills.Count) * 100;
        }

        private float CalculateKeywordMatchScore(HashSet<string> resumeKeywords, HashSet<string> jobKeywords)
        {
            if (!jobKeywords.Any()) return 0;

            int matchingWords = resumeKeywords.Intersect(jobKeywords, StringComparer.OrdinalIgnoreCase).Count();
            return ((float)matchingWords / jobKeywords.Count) * 100;
        }

        private float CalculateExperienceMatch(string resumeText, string jobDescription)
        {
            var resumeYears = ExtractYearsOfExperience(resumeText);
            var requiredYears = ExtractRequiredExperience(jobDescription);

            if (requiredYears == 0) return 100; // No specific requirement
            if (resumeYears == 0) return 50;    // Unknown experience

            if (resumeYears >= requiredYears) return 100;
            if (resumeYears >= requiredYears * 0.7) return 80;
            if (resumeYears >= requiredYears * 0.5) return 60;
            return 30;
        }

        private HashSet<string> ExtractSkills(string text)
        {
            if (string.IsNullOrEmpty(text)) return new HashSet<string>();

            var foundSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var lowerText = text.ToLower();

            // Extract technical skills
            foreach (var skill in _technicalSkills)
            {
                if (lowerText.Contains(skill.ToLower()))
                {
                    foundSkills.Add(skill);
                }
            }

            return foundSkills;
        }

        private int ExtractYearsOfExperience(string text)
        {
            // Look for patterns like "5 years", "3+ years", "2-4 years"
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

        private int ExtractRequiredExperience(string jobDescription)
        {
            // Look for required experience in job description
            var patterns = new[]
            {
                @"(\d+)\+?\s*years?\s+(?:of\s+)?experience\s+required",
                @"minimum\s+(\d+)\s*years?",
                @"(\d+)\s*-\s*\d+\s*years?\s+required",
                @"requires?\s+(\d+)\s*years?"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(jobDescription, pattern, RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int years))
                {
                    return years;
                }
            }

            return 0;
        }

        private HashSet<string> ExtractKeywords(string text)
        {
            if (string.IsNullOrEmpty(text)) return new HashSet<string>();

            // Common stop words to exclude
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by",
                "a", "an", "is", "are", "was", "were", "be", "been", "have", "has", "had",
                "do", "does", "did", "will", "would", "could", "should", "may", "might",
                "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those"
            };

            return text
                .Split(new char[] { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?' },
                       StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 2 && !stopWords.Contains(word))
                .Select(word => word.ToLower().Trim())
                .ToHashSet();
        }

        public List<(string JobTitle, float MatchScore)> FindBestMatches(string resumeText, List<(string JobTitle, string Description)> jobs, int topN = 5)
        {
            var matches = jobs
                .Select(job => new
                {
                    JobTitle = job.JobTitle,
                    MatchScore = CalculateMatchScore(resumeText, job.Description)
                })
                .OrderByDescending(x => x.MatchScore)
                .Take(topN)
                .Select(x => (x.JobTitle, x.MatchScore))
                .ToList();

            return matches;
        }

        private Dictionary<string, HashSet<string>> InitializeSkillSynonyms()
        {
            return new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "JavaScript", new HashSet<string> { "JavaScript", "JS", "ECMAScript", "ES6", "ES2015" } },
                { "C#", new HashSet<string> { "C#", "CSharp", "C Sharp", ".NET", "DotNet" } },
                { "Python", new HashSet<string> { "Python", "Python3", "Py" } },
                { "Java", new HashSet<string> { "Java", "Java8", "Java11", "JDK" } },
                { "React", new HashSet<string> { "React", "ReactJS", "React.js" } },
                { "Angular", new HashSet<string> { "Angular", "AngularJS", "Angular2", "Angular4+" } },
                { "SQL", new HashSet<string> { "SQL", "T-SQL", "MySQL", "PostgreSQL", "SQLServer" } },
                { "Machine Learning", new HashSet<string> { "Machine Learning", "ML", "Artificial Intelligence", "AI" } },
                { "Docker", new HashSet<string> { "Docker", "Containerization", "Containers" } },
                { "Kubernetes", new HashSet<string> { "Kubernetes", "K8s", "Container Orchestration" } },
                { "AWS", new HashSet<string> { "AWS", "Amazon Web Services", "Amazon Cloud" } },
                { "Azure", new HashSet<string> { "Azure", "Microsoft Azure", "Azure Cloud" } },
                { "DevOps", new HashSet<string> { "DevOps", "CI/CD", "Continuous Integration", "Continuous Deployment" } }
            };
        }

        private HashSet<string> InitializeTechnicalSkills()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
                
                // Frameworks & Libraries
                ".NET", "ASP.NET", "Entity Framework", "Spring", "Django", "Flask", "Laravel",
                "CodeIgniter", "Rails", "Express.js", "Symfony",
                
                // Data Science & ML
                "Machine Learning", "Deep Learning", "TensorFlow", "PyTorch", "Scikit-learn",
                "Pandas", "NumPy", "Matplotlib", "Jupyter", "Apache Spark", "Hadoop",
                
                // Mobile Development
                "iOS", "Android", "React Native", "Flutter", "Xamarin", "Ionic",
                
                // Testing
                "Unit Testing", "Integration Testing", "Selenium", "Jest", "Mocha", "Cypress",
                "JUnit", "NUnit", "TestNG",
                
                // Tools & IDEs
                "Visual Studio", "VS Code", "IntelliJ", "Eclipse", "Xcode", "Android Studio",
                "Postman", "Swagger", "JIRA", "Confluence", "Slack"
            };
        }
    }
}
