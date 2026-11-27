using EduHub.Data.Entities;

namespace EduHub.Services.Interfaces;

public interface IQuizService
{
    Task<Quiz?> GetQuizForStudentAsync(int quizId, string studentId);
    Task<QuizAttempt?> SubmitQuizAsync(int quizId, string studentId, Dictionary<int, int> selectedChoices);
    Task<QuizAttempt?> GetQuizAttemptResultAsync(int attemptId, string studentId);
}
