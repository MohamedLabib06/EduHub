using EduHub.Data.Context;
using EduHub.Data.Entities;
using EduHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Services.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly EduHubDbContext _context;

    public EnrollmentService(EduHubDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EnrollStudentAsync(string studentId, int courseId)
    {
        if (string.IsNullOrWhiteSpace(studentId) || courseId <= 0)
            return false;

        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
            return false;

        // Ensure student record exists
        var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);
        if (!studentExists)
        {
            var user = await _context.Users.FindAsync(studentId);
            if (user == null)
                return false;

            _context.Students.Add(new Student { Id = studentId });
            await _context.SaveChangesAsync();
        }

        // Prevent duplicate enrollment
        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (alreadyEnrolled)
            return true;

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            AmountPaid = course.Price
        };

        _context.Enrollments.Add(enrollment);

        // Update Instructor TotalEarnings
        var instructor = await _context.Instructors.FindAsync(course.InstructorId);
        if (instructor != null)
        {
            instructor.TotalEarnings += course.Price;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Enrollment>> GetStudentCoursesAsync(string studentId)
    {
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                    .ThenInclude(i => i.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();
    }

    public async Task<bool> MarkLessonCompletedAsync(string studentId, int lessonId)
    {
        if (string.IsNullOrWhiteSpace(studentId) || lessonId <= 0)
            return false;

        var lesson = await _context.Lessons
            .Include(l => l.Section)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
            return false;

        int courseId = lesson.Section.CourseId;

        // Verify student is enrolled in the course
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (enrollment == null)
            return false;

        // Ensure student profile exists
        var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);
        if (!studentExists)
        {
            _context.Students.Add(new Student { Id = studentId });
            await _context.SaveChangesAsync();
        }

        // Update or insert LessonProgress
        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                StudentId = studentId,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };
            _context.LessonProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Calculate progress percentage for the course
        var allCourseLessonIds = await _context.Sections
            .Where(s => s.CourseId == courseId)
            .SelectMany(s => s.Lessons)
            .Select(l => l.Id)
            .ToListAsync();

        if (allCourseLessonIds.Count > 0)
        {
            var completedCount = await _context.LessonProgresses
                .CountAsync(lp => lp.StudentId == studentId && lp.IsCompleted && allCourseLessonIds.Contains(lp.LessonId));

            // Issue CertificateCode when 100% completed
            if (completedCount >= allCourseLessonIds.Count && string.IsNullOrEmpty(enrollment.CertificateCode))
            {
                enrollment.CertificateCode = $"EDUHUB-{courseId}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                await _context.SaveChangesAsync();
            }
        }

        return true;
    }

    public async Task<bool> AddReviewAsync(string studentId, int courseId, int rating, string? comment)
    {
        if (string.IsNullOrWhiteSpace(studentId) || courseId <= 0)
            return false;

        // Verify enrollment
        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (!isEnrolled)
            return false;

        // Prevent duplicate review
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.StudentId == studentId && r.CourseId == courseId);

        if (existingReview != null)
            return false;

        // Ensure student profile exists
        var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);
        if (!studentExists)
        {
            _context.Students.Add(new Student { Id = studentId });
            await _context.SaveChangesAsync();
        }

        rating = Math.Clamp(rating, 1, 5);

        var review = new Review
        {
            StudentId = studentId,
            CourseId = courseId,
            Rating = rating,
            Comment = comment?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Recalculate Courses.AverageRating
        var allRatings = await _context.Reviews
            .Where(r => r.CourseId == courseId)
            .Select(r => r.Rating)
            .ToListAsync();

        var course = await _context.Courses.FindAsync(courseId);
        if (course != null && allRatings.Count > 0)
        {
            course.AverageRating = Math.Round(allRatings.Average(), 1);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> IsStudentEnrolledAsync(string studentId, int courseId)
    {
        if (string.IsNullOrWhiteSpace(studentId) || courseId <= 0)
            return false;

        return await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task<double> GetCourseProgressPercentageAsync(string studentId, int courseId)
    {
        if (string.IsNullOrWhiteSpace(studentId) || courseId <= 0)
            return 0;

        var allCourseLessonIds = await _context.Sections
            .Where(s => s.CourseId == courseId)
            .SelectMany(s => s.Lessons)
            .Select(l => l.Id)
            .ToListAsync();

        if (allCourseLessonIds.Count == 0)
            return 0;

        var completedCount = await _context.LessonProgresses
            .CountAsync(lp => lp.StudentId == studentId && lp.IsCompleted && allCourseLessonIds.Contains(lp.LessonId));

        return Math.Round(((double)completedCount / allCourseLessonIds.Count) * 100.0, 1);
    }

    public async Task<Course?> GetCourseWithCurriculumAsync(int courseId)
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

    public async Task<List<int>> GetCompletedLessonIdsAsync(string studentId, int courseId)
    {
        if (string.IsNullOrWhiteSpace(studentId) || courseId <= 0)
            return new List<int>();

        var allCourseLessonIds = await _context.Sections
            .Where(s => s.CourseId == courseId)
            .SelectMany(s => s.Lessons)
            .Select(l => l.Id)
            .ToListAsync();

        return await _context.LessonProgresses
            .Where(lp => lp.StudentId == studentId && lp.IsCompleted && allCourseLessonIds.Contains(lp.LessonId))
            .Select(lp => lp.LessonId)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollmentAsync(string studentId, int courseId)
    {
        if (string.IsNullOrWhiteSpace(studentId) || courseId <= 0)
            return null;

        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task<bool> HasStudentReviewedAsync(string studentId, int courseId)
    {
        if (string.IsNullOrWhiteSpace(studentId) || courseId <= 0)
            return false;

        return await _context.Reviews
            .AnyAsync(r => r.StudentId == studentId && r.CourseId == courseId);
    }
}
