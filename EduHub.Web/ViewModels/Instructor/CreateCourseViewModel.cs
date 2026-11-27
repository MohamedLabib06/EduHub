using System.ComponentModel.DataAnnotations;
using EduHub.Data.Entities;
using EduHub.Data.Enums;

namespace EduHub.Web.ViewModels.Instructor;

public class CreateCourseViewModel
{
    [Required(ErrorMessage = "Course title is required.")]
    [MaxLength(200)]
    [Display(Name = "Course Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required]
    [Range(0, 9999.99, ErrorMessage = "Price must be between 0 and 9999.99.")]
    [Display(Name = "Price ($)")]
    public decimal Price { get; set; }

    [Required]
    [Display(Name = "Difficulty Level")]
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    [Required]
    [Display(Name = "Initial Status")]
    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();
}
