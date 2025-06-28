using System.ComponentModel.DataAnnotations;

namespace ResumeMatcher.Models
{
    public class JobPost
    {
        public int Id { get; set; }

        [Required]
        public string? JobTitle { get; set; }

        [Required]
        public string? Description { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.Now;
    }
}
