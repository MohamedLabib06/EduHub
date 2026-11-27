namespace EduHub.Web.ViewModels.Quiz;

public class TakeQuizViewModel
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public decimal PassingScore { get; set; }
    public List<QuizQuestionItemViewModel> Questions { get; set; } = new();
    public Dictionary<int, int> SelectedChoices { get; set; } = new();
}

public class QuizQuestionItemViewModel
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<QuizChoiceItemViewModel> Choices { get; set; } = new();
}

public class QuizChoiceItemViewModel
{
    public int ChoiceId { get; set; }
    public string ChoiceText { get; set; } = string.Empty;
}
