using System.ComponentModel.DataAnnotations;
using EduHub.Data.Enums;

namespace EduHub.Data.Entities;

public class Course
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public CourseLevel Level { get; set; }

    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    public double AverageRating { get; set; }

    // Foreign Keys
    [Required]
    public string InstructorId { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    // Navigation Properties
    public Instructor Instructor { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}