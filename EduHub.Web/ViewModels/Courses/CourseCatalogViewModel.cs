using EduHub.Data.Entities;
using EduHub.Data.Enums;

namespace EduHub.Web.ViewModels.Courses;

public class CourseCatalogViewModel
{
    public IEnumerable<Course> Courses { get; set; } = Enumerable.Empty<Course>();

    public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();

    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public CourseLevel? Level { get; set; }
}
