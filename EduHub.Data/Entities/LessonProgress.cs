using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class LessonProgress
{
    public int Id { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Foreign Keys
    [Required]
    public string StudentId { get; set; } = string.Empty;

    public int LessonId { get; set; }

    // Navigation Properties
    public Student Student { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}