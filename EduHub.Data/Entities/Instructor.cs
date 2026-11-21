using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduHub.Data.Entities;

public class Instructor
{
    [Key, ForeignKey(nameof(User))]
    public string Id { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Headline { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public decimal TotalEarnings { get; set; }

    // Navigation Properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}