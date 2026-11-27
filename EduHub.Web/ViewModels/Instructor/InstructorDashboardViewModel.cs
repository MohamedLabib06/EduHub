using EduHub.Data.Entities;

namespace EduHub.Web.ViewModels.Instructor;

public class InstructorDashboardViewModel
{
    public string InstructorName { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public decimal TotalEarnings { get; set; }
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public IEnumerable<Course> Courses { get; set; } = Enumerable.Empty<Course>();
}
