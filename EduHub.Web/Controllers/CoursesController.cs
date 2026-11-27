using EduHub.Data.Enums;
using EduHub.Services.Interfaces;
using EduHub.Web.ViewModels.Courses;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.Web.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, CourseLevel? level)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var normalizedCategoryId = categoryId.HasValue && categoryId.Value > 0 ? categoryId : null;
        var normalizedLevel = level.HasValue && Enum.IsDefined(level.Value) ? level : null;

        var courses = await _courseService.GetAllPublishedCoursesAsync(
            normalizedSearch,
            normalizedCategoryId);

        if (normalizedLevel.HasValue)
        {
            courses = courses.Where(c => c.Level == normalizedLevel.Value).ToList();
        }

        var categories = await _courseService.GetAllCategoriesAsync();

        var model = new CourseCatalogViewModel
        {
            Courses = courses,
            Categories = categories,
            Search = normalizedSearch,
            CategoryId = normalizedCategoryId,
            Level = normalizedLevel
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var course = await _courseService.GetCourseDetailsAsync(id);

        // The service does not filter by status, so published-only access
        // is enforced here before the course is sent to the view.
        if (course == null || course.Status != CourseStatus.Published)
        {
            return NotFound();
        }

        return View(course);
    }
}
