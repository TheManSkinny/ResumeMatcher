using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ResumeMatcher.Services
{
    public static class MatchingService
    {
        public static int CalculateMatchScore(string resumeText, string jobDescription)
        {
            if (string.IsNullOrWhiteSpace(resumeText) || string.IsNullOrWhiteSpace(jobDescription))
                return 0;

            // Normalize both texts
            var resumeWords = ExtractKeywords(resumeText);
            var jobWords = ExtractKeywords(jobDescription);

            if (jobWords.Count == 0)
                return 0;

            // Calculate intersection
            int matchedWords = resumeWords.Intersect(jobWords).Count();
            int score = (int)((matchedWords / (double)jobWords.Count) * 100);

            return score;
        }

        private static HashSet<string> ExtractKeywords(string text)
        {
            var words = Regex.Split(text.ToLower(), @"[\s\p{P}-]+")
                             .Where(w => w.Length > 2 && !IsStopWord(w));
            return new HashSet<string>(words);
        }

        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "the", "and", "for", "you", "with", "that", "are", "but", "have", "not", "all",
            "your", "this", "from", "can", "our", "will", "job", "title", "more", "use", "any"
        };

        private static bool IsStopWord(string word)
        {
            return StopWords.Contains(word);
        }
    }
}
// This service provides methods to calculate the match score between a resume and a job description.
// It extracts keywords from both texts, normalizes them, and calculates the score based on the intersection of keywords.
// The score is a percentage of matched keywords from the job description found in the resume.