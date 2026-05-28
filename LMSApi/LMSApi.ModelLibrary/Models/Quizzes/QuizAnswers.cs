namespace LMSApi.ModelLibrary.Models
{
    public class QuizAnswers
    {
        public int Id{get;set;}
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; } // For multiple choice and true/false questions
        public bool IsCorrect { get; set; } // To indicate if the selected answer is correct
    }
}