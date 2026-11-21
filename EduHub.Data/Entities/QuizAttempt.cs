using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class QuizAttempt
{
    public int Id { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }

    public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    [Required]
    public string StudentId { get; set; } = string.Empty;

    public int QuizId { get; set; }

    // Navigation Properties
    public Student Student { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
}