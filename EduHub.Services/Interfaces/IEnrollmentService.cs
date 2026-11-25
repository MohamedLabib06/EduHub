using EduHub.Data.Entities;

namespace EduHub.Services.Interfaces;

public interface IEnrollmentService
{
    Task<bool> EnrollStudentAsync(string studentId, int courseId);
    Task<IEnumerable<Enrollment>> GetStudentCoursesAsync(string studentId);
    Task<bool> MarkLessonCompletedAsync(string studentId, int lessonId);
    Task<bool> AddReviewAsync(string studentId, int courseId, int rating, string? comment);

    // Queries and helpers for student learning flow
    Task<bool> IsStudentEnrolledAsync(string studentId, int courseId);
    Task<double> GetCourseProgressPercentageAsync(string studentId, int courseId);
    Task<Course?> GetCourseWithCurriculumAsync(int courseId);
    Task<List<int>> GetCompletedLessonIdsAsync(string studentId, int courseId);
    Task<Enrollment?> GetEnrollmentAsync(string studentId, int courseId);
    Task<bool> HasStudentReviewedAsync(string studentId, int courseId);
}
