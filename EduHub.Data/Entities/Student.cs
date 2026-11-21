using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduHub.Data.Entities;

public class Student
{
    [Key, ForeignKey(nameof(User))]
    public string Id { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProfileImage { get; set; }

    [MaxLength(500)]
    public string? Interests { get; set; }

    // Navigation Properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}