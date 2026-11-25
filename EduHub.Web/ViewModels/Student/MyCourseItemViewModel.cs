namespace EduHub.Web.ViewModels.Student;

public class MyCourseItemViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime EnrolledAt { get; set; }
    public double ProgressPercentage { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int TotalLessonsCount { get; set; }
    public string? CertificateCode { get; set; }
    public bool IsCompleted => ProgressPercentage >= 100.0;
}
