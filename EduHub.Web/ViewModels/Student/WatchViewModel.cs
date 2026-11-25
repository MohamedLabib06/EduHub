using EduHub.Data.Entities;

namespace EduHub.Web.ViewModels.Student;

public class WatchViewModel
{
    public Course Course { get; set; } = null!;
    public Lesson CurrentLesson { get; set; } = null!;
    public Lesson? PreviousLesson { get; set; }
    public Lesson? NextLesson { get; set; }
    public HashSet<int> CompletedLessonIds { get; set; } = new();
    public double ProgressPercentage { get; set; }
    public string? CertificateCode { get; set; }
    public bool HasReviewed { get; set; }
    public Review? StudentReview { get; set; }
    public string VideoEmbedUrl { get; set; } = string.Empty;

    public bool IsCurrentLessonCompleted => CompletedLessonIds.Contains(CurrentLesson.Id);
}
