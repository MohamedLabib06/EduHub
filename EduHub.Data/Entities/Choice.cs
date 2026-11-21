using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class Choice
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string ChoiceText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    // Foreign Key
    public int QuestionId { get; set; }

    // Navigation Properties
    public Question Question { get; set; } = null!;
}