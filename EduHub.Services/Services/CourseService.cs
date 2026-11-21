using EduHub.Data.Context;
using EduHub.Data.Entities;
using EduHub.Data.Enums;
using EduHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Services.Services;

public class CourseService : ICourseService
{
    private readonly EduHubDbContext _context;

    public CourseService(EduHubDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Course>> GetAllPublishedCoursesAsync(string? searchKeyword = null, int? categoryId = null)
    {
        var query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .ThenInclude(i => i.User)
            .Include(c => c.Sections)
            .ThenInclude(s => s.Lessons)
            .Where(c => c.Status == CourseStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(c => c.Title.Contains(searchKeyword) || c.Category.Name.Contains(searchKeyword));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        return await query.OrderByDescending(c => c.AverageRating).ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories.Include(c => c.Courses).ToListAsync();
    }

    public async Task<Course?> GetCourseDetailsAsync(int courseId)
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .ThenInclude(i => i.User)
            .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
            .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
            .Include(c => c.Reviews)
            .ThenInclude(r => r.Student)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }
}