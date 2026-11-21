using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class Question
{
    public int Id { get; set; }

    [Required, MaxLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    public int Points { get; set; } = 1;

    // Foreign Key
    public int QuizId { get; set; }

    // Navigation Properties
    public Quiz Quiz { get; set; } = null!;
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
}