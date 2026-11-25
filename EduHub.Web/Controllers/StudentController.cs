using System.Security.Claims;
using EduHub.Data.Entities;
using EduHub.Services.Interfaces;
using EduHub.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentController(
        IEnrollmentService enrollmentService,
        UserManager<ApplicationUser> userManager)
    {
        _enrollmentService = enrollmentService;
        _userManager = userManager;
    }

    private string GetCurrentUserId()
    {
        return _userManager.GetUserId(User) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Challenge();

        var success = await _enrollmentService.EnrollStudentAsync(studentId, courseId);
        if (!success)
        {
            TempData["ErrorMessage"] = "Could not enroll in this course. It may not exist.";
            return RedirectToAction("Index", "Home");
        }

        TempData["SuccessMessage"] = "Congratulations! You have successfully enrolled in this course.";
        return RedirectToAction(nameof(Watch), new { courseId });
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Challenge();

        var enrollments = await _enrollmentService.GetStudentCoursesAsync(studentId);
        var viewModelList = new List<MyCourseItemViewModel>();

        foreach (var enrollment in enrollments)
        {
            var progress = await _enrollmentService.GetCourseProgressPercentageAsync(studentId, enrollment.CourseId);
            var totalLessons = enrollment.Course.Sections.SelectMany(s => s.Lessons).Count();
            var completedLessons = (int)Math.Round((progress / 100.0) * totalLessons);

            viewModelList.Add(new MyCourseItemViewModel
            {
                CourseId = enrollment.CourseId,
                CourseTitle = enrollment.Course.Title,
                CategoryName = enrollment.Course.Category?.Name ?? "General",
                InstructorName = $"{enrollment.Course.Instructor?.User?.FirstName} {enrollment.Course.Instructor?.User?.LastName}".Trim(),
                Level = enrollment.Course.Level.ToString(),
                Price = enrollment.AmountPaid,
                EnrolledAt = enrollment.EnrolledAt,
                ProgressPercentage = progress,
                CompletedLessonsCount = completedLessons,
                TotalLessonsCount = totalLessons,
                CertificateCode = enrollment.CertificateCode
            });
        }

        return View(viewModelList);
    }

    [HttpGet]
    public async Task<IActionResult> Watch(int courseId, int? lessonId)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Challenge();

        // 1. Verify student is enrolled
        var isEnrolled = await _enrollmentService.IsStudentEnrolledAsync(studentId, courseId);
        if (!isEnrolled)
        {
            TempData["ErrorMessage"] = "You must be enrolled in this course to watch its content.";
            return RedirectToAction("Index", "Home");
        }

        // 2. Load course curriculum
        var course = await _enrollmentService.GetCourseWithCurriculumAsync(courseId);
        if (course == null)
            return NotFound();

        var allLessons = course.Sections
            .OrderBy(s => s.OrderIndex)
            .SelectMany(s => s.Lessons.OrderBy(l => l.OrderIndex))
            .ToList();

        if (allLessons.Count == 0)
        {
            ViewBag.ErrorMessage = "This course does not have any lessons available yet.";
            return View("NoLessons", course);
        }

        var completedIds = await _enrollmentService.GetCompletedLessonIdsAsync(studentId, courseId);
        var completedSet = new HashSet<int>(completedIds);

        // 3. Determine current lesson
        Lesson? currentLesson = null;
        if (lessonId.HasValue)
        {
            currentLesson = allLessons.FirstOrDefault(l => l.Id == lessonId.Value);
        }

        // If not specified or not found, go to the first uncompleted lesson or the first lesson
        currentLesson ??= allLessons.FirstOrDefault(l => !completedSet.Contains(l.Id)) ?? allLessons.First();

        int currentIndex = allLessons.FindIndex(l => l.Id == currentLesson.Id);
        var prevLesson = currentIndex > 0 ? allLessons[currentIndex - 1] : null;
        var nextLesson = currentIndex >= 0 && currentIndex < allLessons.Count - 1 ? allLessons[currentIndex + 1] : null;

        var progress = await _enrollmentService.GetCourseProgressPercentageAsync(studentId, courseId);
        var enrollment = await _enrollmentService.GetEnrollmentAsync(studentId, courseId);
        var hasReviewed = await _enrollmentService.HasStudentReviewedAsync(studentId, courseId);
        var studentReview = course.Reviews.FirstOrDefault(r => r.StudentId == studentId);

        var viewModel = new WatchViewModel
        {
            Course = course,
            CurrentLesson = currentLesson,
            PreviousLesson = prevLesson,
            NextLesson = nextLesson,
            CompletedLessonIds = completedSet,
            ProgressPercentage = progress,
            CertificateCode = enrollment?.CertificateCode,
            HasReviewed = hasReviewed,
            StudentReview = studentReview,
            VideoEmbedUrl = FormatVideoUrl(currentLesson.VideoUrl)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkLessonCompleted(int lessonId, int courseId)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Challenge();

        var success = await _enrollmentService.MarkLessonCompletedAsync(studentId, lessonId);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var newProgress = await _enrollmentService.GetCourseProgressPercentageAsync(studentId, courseId);
            var enrollment = await _enrollmentService.GetEnrollmentAsync(studentId, courseId);
            return Json(new
            {
                success = success,
                progress = newProgress,
                certificateCode = enrollment?.CertificateCode
            });
        }

        if (success)
        {
            TempData["SuccessMessage"] = "Lesson marked as completed!";
        }

        return RedirectToAction(nameof(Watch), new { courseId, lessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int courseId, int rating, string? comment)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Challenge();

        if (rating < 1 || rating > 5)
        {
            TempData["ErrorMessage"] = "Please provide a valid rating between 1 and 5 stars.";
            return RedirectToAction(nameof(Watch), new { courseId });
        }

        var success = await _enrollmentService.AddReviewAsync(studentId, courseId, rating, comment);
        if (success)
        {
            TempData["SuccessMessage"] = "Thank you! Your review has been submitted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "You have already reviewed this course or are not authorized.";
        }

        return RedirectToAction(nameof(Watch), new { courseId });
    }

    private static string FormatVideoUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "";

        // Standard YouTube watch link: https://www.youtube.com/watch?v=VIDEO_ID
        if (url.Contains("youtube.com/watch?v="))
        {
            var parts = url.Split("watch?v=");
            if (parts.Length > 1)
            {
                var videoId = parts[1].Split('&')[0];
                return $"https://www.youtube.com/embed/{videoId}?autoplay=0&rel=0";
            }
        }

        // Short YouTube link: https://youtu.be/VIDEO_ID
        if (url.Contains("youtu.be/"))
        {
            var parts = url.Split("youtu.be/");
            if (parts.Length > 1)
            {
                var videoId = parts[1].Split('?')[0];
                return $"https://www.youtube.com/embed/{videoId}?autoplay=0&rel=0";
            }
        }

        return url;
    }
}
