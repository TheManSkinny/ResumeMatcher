namespace ResumeMatcher.Models
{
    public class MatchResult
    {
        public int Id { get; set; }
        public int CVId { get; set; }
        public int JobPostId { get; set; }
        public float MatchScore { get; set; }
    }
}
