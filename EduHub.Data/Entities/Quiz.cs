using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class Quiz
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public decimal PassingScore { get; set; }

    // Foreign Key
    public int SectionId { get; set; }

    // Navigation Properties
    public Section Section { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}