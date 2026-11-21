using System.ComponentModel.DataAnnotations;

namespace EduHub.Data.Entities;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation Properties
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}