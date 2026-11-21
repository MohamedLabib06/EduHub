using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class Lesson
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string VideoUrl { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; }

    public int OrderIndex { get; set; }

    // Foreign Key
    public int SectionId { get; set; }

    // Navigation Properties
    public Section Section { get; set; } = null!;
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}