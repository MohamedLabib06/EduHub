using EduHub.Data.Entities;

namespace EduHub.Services.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<Course>> GetAllPublishedCoursesAsync(string? searchKeyword = null, int? categoryId = null);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Course?> GetCourseDetailsAsync(int courseId);
}