using System.Security.Claims;
using System.Text.Json;
using EduHub.Data.Entities;
using EduHub.Services.Interfaces;
using EduHub.Web.ViewModels.Quiz;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.Web.Controllers;

[Authorize(Roles = "Student")]
public class QuizController : Controller
{
    private readonly IQuizService _quizService;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuizController(
        IQuizService quizService,
        UserManager<ApplicationUser> userManager)
    {
        _quizService = quizService;
        _userManager = userManager;
    }

    private string GetCurrentUserId()
    {
        return _userManager.GetUserId(User) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> TakeQuiz(int id)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Challenge();

        var quiz = await _quizService.GetQuizForStudentAsync(id, studentId);
        if (quiz == null)
        {
            TempData["ErrorMessage"] = "Quiz not found or you must be enrolled in the course to take this quiz.";
            return RedirectToAction("MyCourses", "Student");
        }

        if (!quiz.Questions.Any())
        {
            TempData["ErrorMessage"] = "This quiz does not have any questions yet.";
            return RedirectToAction("Watch", "Student", new { courseId = quiz.Section.CourseId });
        }

        // Map strictly to ViewModel WITHOUT IsCorrect so answers are never leaked before submission
        var viewModel = new TakeQuizViewModel
        {
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            CourseId = quiz.Section.CourseId,
            CourseTitle = quiz.Section.Course.Title,
            SectionTitle = quiz.Section.Title,
            PassingScore = quiz.PassingScore,
            Questions = quiz.Questions.Select(q => new QuizQuestionItemViewModel
            {
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                Points = q.Points,
                Choices = q.Choices.Select(c => new QuizChoiceItemViewModel
                {
                    ChoiceId = c.Id,
                    ChoiceText = c.ChoiceText
                }).ToList()
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(TakeQuizViewModel model)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Challenge();

        var selected = model.SelectedChoices ?? new Dictionary<int, int>();
        var attempt = await _quizService.SubmitQuizAsync(model.QuizId, studentId, selected);
        if (attempt == null)
        {
            TempData["ErrorMessage"] = "Unable to submit quiz. Ensure you are enrolled in the course.";
            return RedirectToAction("MyCourses", "Student");
        }

        TempData["SelectedChoicesJson"] = JsonSerializer.Serialize(selected);
        return RedirectToAction(nameof(Result), new { id = attempt.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Result(int id)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Challenge();

        var attempt = await _quizService.GetQuizAttemptResultAsync(id, studentId);
        if (attempt == null)
            return NotFound();

        Dictionary<int, int>? selectedChoices = null;
        if (TempData["SelectedChoicesJson"] is string json && !string.IsNullOrEmpty(json))
        {
            selectedChoices = JsonSerializer.Deserialize<Dictionary<int, int>>(json);
        }

        ViewBag.SelectedChoices = selectedChoices ?? new Dictionary<int, int>();
        return View(attempt);
    }
}
