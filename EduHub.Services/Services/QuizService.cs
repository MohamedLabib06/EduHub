using EduHub.Data.Context;
using EduHub.Data.Entities;
using EduHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Services.Services;

public class QuizService : IQuizService
{
    private readonly EduHubDbContext _context;

    public QuizService(EduHubDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetQuizForStudentAsync(int quizId, string studentId)
    {
        if (quizId <= 0 || string.IsNullOrWhiteSpace(studentId))
            return null;

        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Section)
                .ThenInclude(s => s.Course)
            .Include(q => q.Questions)
                .ThenInclude(qn => qn.Choices)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
            return null;

        bool isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == quiz.Section.CourseId);

        if (!isEnrolled)
            return null;

        return quiz;
    }

    public async Task<QuizAttempt?> SubmitQuizAsync(int quizId, string studentId, Dictionary<int, int> selectedChoices)
    {
        if (quizId <= 0 || string.IsNullOrWhiteSpace(studentId))
            return null;

        var quiz = await _context.Quizzes
            .Include(q => q.Section)
            .Include(q => q.Questions)
                .ThenInclude(qn => qn.Choices)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null || !quiz.Questions.Any())
            return null;

        bool isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == quiz.Section.CourseId);

        if (!isEnrolled)
            return null;

        var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);
        if (!studentExists)
        {
            var user = await _context.Users.FindAsync(studentId);
            if (user == null)
                return null;

            _context.Students.Add(new Student { Id = studentId });
            await _context.SaveChangesAsync();
        }

        int totalPoints = 0;
        int earnedPoints = 0;

        foreach (var question in quiz.Questions)
        {
            int qPoints = question.Points > 0 ? question.Points : 1;
            totalPoints += qPoints;

            if (selectedChoices.TryGetValue(question.Id, out int selectedChoiceId))
            {
                var chosen = question.Choices.FirstOrDefault(c => c.Id == selectedChoiceId);
                if (chosen != null && chosen.IsCorrect)
                {
                    earnedPoints += qPoints;
                }
            }
        }

        decimal score = totalPoints > 0
            ? Math.Round((decimal)earnedPoints / totalPoints * 100m, 2)
            : 0m;

        bool isPassed = score >= quiz.PassingScore;

        var attempt = new QuizAttempt
        {
            StudentId = studentId,
            QuizId = quizId,
            Score = score,
            IsPassed = isPassed,
            AttemptDate = DateTime.UtcNow
        };

        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return attempt;
    }

    public async Task<QuizAttempt?> GetQuizAttemptResultAsync(int attemptId, string studentId)
    {
        if (attemptId <= 0 || string.IsNullOrWhiteSpace(studentId))
            return null;

        return await _context.QuizAttempts
            .AsNoTracking()
            .Include(qa => qa.Quiz)
                .ThenInclude(q => q.Section)
                    .ThenInclude(s => s.Course)
            .Include(qa => qa.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(qn => qn.Choices)
            .FirstOrDefaultAsync(qa => qa.Id == attemptId && qa.StudentId == studentId);
    }
}
