using EduHub.Data.Entities;
using EduHub.Data.Enums;

namespace EduHub.Services.Interfaces;

public interface IInstructorService
{
    Task<Instructor?> GetInstructorDashboardDataAsync(string instructorId);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Course?> CreateCourseAsync(string instructorId, string title, int categoryId, decimal price, CourseLevel level, CourseStatus status);
    Task<Course?> GetCourseForManagementAsync(int courseId, string instructorId);
    Task<bool> UpdateCourseStatusAsync(int courseId, string instructorId, CourseStatus status);
    Task<Section?> AddSectionAsync(int courseId, string instructorId, string title);
    Task<Lesson?> AddLessonAsync(int sectionId, string instructorId, string title, string videoUrl, int durationMinutes);
    Task<Quiz?> AddQuizAsync(int sectionId, string instructorId, string title, decimal passingScore);
    Task<Question?> AddQuestionWithChoicesAsync(int quizId, string instructorId, string questionText, int points, List<(string Text, bool IsCorrect)> choices);
}
