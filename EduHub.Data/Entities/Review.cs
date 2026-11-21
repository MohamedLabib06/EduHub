using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class Review
{
    public int Id { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    [Required]
    public string StudentId { get; set; } = string.Empty;

    public int CourseId { get; set; }

    // Navigation Properties
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}