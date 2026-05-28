namespace LMSApi.ModelLibrary.Models
{
    public class QuizOptions
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; } // For multiple choice and true/false questions
        
        // Navigation property
        public QuizQuestions Question { get; set; }
    }
}