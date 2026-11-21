using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class Section
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }

    // Foreign Key
    public int CourseId { get; set; }

    // Navigation Properties
    public Course Course { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}