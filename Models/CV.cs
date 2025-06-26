namespace ResumeMatcher.Models
{
    public class CV
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ParsedText { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
