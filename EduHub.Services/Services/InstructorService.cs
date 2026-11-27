using EduHub.Data.Context;
using EduHub.Data.Entities;
using EduHub.Data.Enums;
using EduHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Services.Services;

public class InstructorService : IInstructorService
{
    private readonly EduHubDbContext _context;

    public InstructorService(EduHubDbContext context)
    {
        _context = context;
    }

    public async Task<Instructor?> GetInstructorDashboardDataAsync(string instructorId)
    {
        if (string.IsNullOrWhiteSpace(instructorId))
            return null;

        var instructor = await _context.Instructors
            .Include(i => i.User)
            .Include(i => i.Courses)
                .ThenInclude(c => c.Category)
            .Include(i => i.Courses)
                .ThenInclude(c => c.Enrollments)
            .Include(i => i.Courses)
                .ThenInclude(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
            .Include(i => i.Courses)
                .ThenInclude(c => c.Sections)
                    .ThenInclude(s => s.Quizzes)
            .FirstOrDefaultAsync(i => i.Id == instructorId);

        if (instructor == null)
        {
            var user = await _context.Users.FindAsync(instructorId);
            if (user == null)
                return null;

            instructor = new Instructor
            {
                Id = instructorId,
                Headline = "EduHub Instructor",
                Bio = string.Empty,
                TotalEarnings = 0m
            };
            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            return await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                .FirstOrDefaultAsync(i => i.Id == instructorId);
        }

        return instructor;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Course?> CreateCourseAsync(string instructorId, string title, int categoryId, decimal price, CourseLevel level, CourseStatus status)
    {
        if (string.IsNullOrWhiteSpace(instructorId) || string.IsNullOrWhiteSpace(title))
            return null;

        var instructorExists = await _context.Instructors.AnyAsync(i => i.Id == instructorId);
        if (!instructorExists)
        {
            var user = await _context.Users.FindAsync(instructorId);
            if (user == null)
                return null;

            _context.Instructors.Add(new Instructor
            {
                Id = instructorId,
                Headline = "EduHub Instructor",
                TotalEarnings = 0m
            });
            await _context.SaveChangesAsync();
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
            return null;

        if (status != CourseStatus.Draft && status != CourseStatus.Published)
            status = CourseStatus.Draft;

        var course = new Course
        {
            Title = title.Trim(),
            CategoryId = categoryId,
            Price = Math.Max(0, price),
            Level = level,
            Status = status,
            AverageRating = 0,
            InstructorId = instructorId
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<Course?> GetCourseForManagementAsync(int courseId, string instructorId)
    {
        if (courseId <= 0 || string.IsNullOrWhiteSpace(instructorId))
            return null;

        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Enrollments)
            .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
            .Include(c => c.Sections)
                .ThenInclude(s => s.Quizzes)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qn => qn.Choices)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);
    }

    public async Task<bool> UpdateCourseStatusAsync(int courseId, string instructorId, CourseStatus status)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null)
            return false;

        course.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Section?> AddSectionAsync(int courseId, string instructorId, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        var course = await _context.Courses
            .Include(c => c.Sections)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null)
            return null;

        int nextOrder = course.Sections.Any() ? course.Sections.Max(s => s.OrderIndex) + 1 : 1;

        var section = new Section
        {
            CourseId = courseId,
            Title = title.Trim(),
            OrderIndex = nextOrder
        };

        _context.Sections.Add(section);
        await _context.SaveChangesAsync();
        return section;
    }

    public async Task<Lesson?> AddLessonAsync(int sectionId, string instructorId, string title, string videoUrl, int durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(videoUrl))
            return null;

        var section = await _context.Sections
            .Include(s => s.Course)
            .Include(s => s.Lessons)
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.Course.InstructorId == instructorId);

        if (section == null)
            return null;

        int nextOrder = section.Lessons.Any() ? section.Lessons.Max(l => l.OrderIndex) + 1 : 1;

        var lesson = new Lesson
        {
            SectionId = sectionId,
            Title = title.Trim(),
            VideoUrl = videoUrl.Trim(),
            Duration = TimeSpan.FromMinutes(Math.Max(1, durationMinutes)),
            OrderIndex = nextOrder
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();
        return lesson;
    }

    public async Task<Quiz?> AddQuizAsync(int sectionId, string instructorId, string title, decimal passingScore)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        var section = await _context.Sections
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.Course.InstructorId == instructorId);

        if (section == null)
            return null;

        var quiz = new Quiz
        {
            SectionId = sectionId,
            Title = title.Trim(),
            PassingScore = Math.Clamp(passingScore, 0m, 100m)
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();
        return quiz;
    }

    public async Task<Question?> AddQuestionWithChoicesAsync(int quizId, string instructorId, string questionText, int points, List<(string Text, bool IsCorrect)> choices)
    {
        if (string.IsNullOrWhiteSpace(questionText) || choices == null)
            return null;

        var validChoices = choices
            .Where(c => !string.IsNullOrWhiteSpace(c.Text))
            .Select(c => (Text: c.Text.Trim(), c.IsCorrect))
            .ToList();

        if (validChoices.Count < 2 || !validChoices.Any(c => c.IsCorrect))
            return null;

        var quiz = await _context.Quizzes
            .Include(q => q.Section)
                .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(q => q.Id == quizId && q.Section.Course.InstructorId == instructorId);

        if (quiz == null)
            return null;

        var question = new Question
        {
            QuizId = quizId,
            QuestionText = questionText.Trim(),
            Points = Math.Max(1, points),
            Choices = validChoices.Select(c => new Choice
            {
                ChoiceText = c.Text,
                IsCorrect = c.IsCorrect
            }).ToList()
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }
}
