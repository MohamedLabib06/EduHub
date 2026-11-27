using System.Security.Claims;
using EduHub.Data.Entities;
using EduHub.Data.Enums;
using EduHub.Services.Interfaces;
using EduHub.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.Web.Controllers;

[Authorize(Roles = "Instructor")]
public class InstructorController : Controller
{
    private readonly IInstructorService _instructorService;
    private readonly UserManager<ApplicationUser> _userManager;

    public InstructorController(
        IInstructorService instructorService,
        UserManager<ApplicationUser> userManager)
    {
        _instructorService = instructorService;
        _userManager = userManager;
    }

    private string GetCurrentUserId()
    {
        return _userManager.GetUserId(User) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var instructorId = GetCurrentUserId();
        if (string.IsNullOrEmpty(instructorId))
            return Challenge();

        var instructor = await _instructorService.GetInstructorDashboardDataAsync(instructorId);
        if (instructor == null)
            return NotFound();

        var totalStudents = instructor.Courses
            .SelectMany(c => c.Enrollments)
            .Select(e => e.StudentId)
            .Distinct()
            .Count();

        var viewModel = new InstructorDashboardViewModel
        {
            InstructorName = $"{instructor.User.FirstName} {instructor.User.LastName}".Trim(),
            Headline = instructor.Headline,
            TotalEarnings = instructor.TotalEarnings,
            TotalStudents = totalStudents,
            TotalCourses = instructor.Courses.Count,
            PublishedCourses = instructor.Courses.Count(c => c.Status == CourseStatus.Published),
            Courses = instructor.Courses.OrderByDescending(c => c.Id)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CreateCourse()
    {
        var viewModel = new CreateCourseViewModel
        {
            Categories = await _instructorService.GetAllCategoriesAsync()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CreateCourseViewModel model)
    {
        var instructorId = GetCurrentUserId();
        if (string.IsNullOrEmpty(instructorId))
            return Challenge();

        if (!ModelState.IsValid)
        {
            model.Categories = await _instructorService.GetAllCategoriesAsync();
            return View(model);
        }

        var course = await _instructorService.CreateCourseAsync(
            instructorId,
            model.Title,
            model.CategoryId,
            model.Price,
            model.Level,
            model.Status);

        if (course == null)
        {
            ModelState.AddModelError(string.Empty, "Could not create course. Please check the selected category.");
            model.Categories = await _instructorService.GetAllCategoriesAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = $"Course \"{course.Title}\" created successfully! Now add sections, lessons, and quizzes.";
        return RedirectToAction(nameof(ManageCurriculum), new { id = course.Id });
    }

    [HttpGet]
    public async Task<IActionResult> ManageCurriculum(int id)
    {
        var instructorId = GetCurrentUserId();
        if (string.IsNullOrEmpty(instructorId))
            return Challenge();

        var course = await _instructorService.GetCourseForManagementAsync(id, instructorId);
        if (course == null)
            return NotFound();

        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int courseId, CourseStatus status)
    {
        var instructorId = GetCurrentUserId();
        if (string.IsNullOrEmpty(instructorId))
            return Challenge();

        var updated = await _instructorService.UpdateCourseStatusAsync(courseId, instructorId, status);
        if (!updated)
            return NotFound();

        TempData["SuccessMessage"] = $"Course status updated to {status}.";
        return RedirectToAction(nameof(ManageCurriculum), new { id = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSection(int courseId, string title)
    {
        var instructorId = GetCurrentUserId();
        if (string.IsNullOrEmpty(instructorId))
            return Challenge();

        var section = await _instructorService.AddSectionAsync(courseId, instructorId, title);
        if (section == null)
        {
            TempData["ErrorMessage"] = "Could not add section. Ensure the title is not empty and you own this course.";
        }
        else
        {
            TempData["SuccessMessage"] = $"Section \"{section.Title}\" added.";
        }

        return RedirectToAction(nameof(ManageCurriculum), new { id = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLesson(int courseId, int sectionId, string title, string videoUrl, int durationMinutes)
    {
        var instructorId = GetCurrentUserId();
        if (string.IsNullOrEmpty(instructorId))
            return Challenge();

        var lesson = await _instructorService.AddLessonAsync(sectionId, instructorId, title, videoUrl, durationMinutes);
        if (lesson == null)
        {
            TempData["ErrorMessage"] = "Could not add lesson. Check all fields and course ownership.";
        }
        else
        {
            TempData["SuccessMessage"] = $"Lesson \"{lesson.Title}\" added.";
        }

        return RedirectToAction(nameof(ManageCurriculum), new { id = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuiz(int courseId, int sectionId, string title, decimal passingScore)
    {
        var instructorId = GetCurrentUserId();
        if (string.IsNullOrEmpty(instructorId))
            return Challenge();

        var quiz = await _instructorService.AddQuizAsync(sectionId, instructorId, title, passingScore);
        if (quiz == null)
        {
            TempData["ErrorMessage"] = "Could not add quiz. Check the quiz title and passing score.";
        }
        else
        {
            TempData["SuccessMessage"] = $"Quiz \"{quiz.Title}\" added.";
        }

        return RedirectToAction(nameof(ManageCurriculum), new { id = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(int courseId, int quizId, string questionText, int points, List<string> choiceTexts, int correctIndex)
    {
        var instructorId = GetCurrentUserId();
        if (string.IsNullOrEmpty(instructorId))
            return Challenge();

        var choices = new List<(string Text, bool IsCorrect)>();
        for (int i = 0; i < choiceTexts.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(choiceTexts[i]))
            {
                choices.Add((choiceTexts[i].Trim(), i == correctIndex));
            }
        }

        var question = await _instructorService.AddQuestionWithChoicesAsync(quizId, instructorId, questionText, points, choices);
        if (question == null)
        {
            TempData["ErrorMessage"] = "Could not add question. Provide question text, at least 2 choices, and select a non-empty correct answer.";
        }
        else
        {
            TempData["SuccessMessage"] = "Question and choices added to quiz.";
        }

        return RedirectToAction(nameof(ManageCurriculum), new { id = courseId });
    }
}
