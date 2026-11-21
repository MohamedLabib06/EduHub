using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class Enrollment
{
    public int Id { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public decimal AmountPaid { get; set; }

    [MaxLength(50)]
    public string? CertificateCode { get; set; }

    // Foreign Keys
    [Required]
    public string StudentId { get; set; } = string.Empty;

    public int CourseId { get; set; }

    // Navigation Properties
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}