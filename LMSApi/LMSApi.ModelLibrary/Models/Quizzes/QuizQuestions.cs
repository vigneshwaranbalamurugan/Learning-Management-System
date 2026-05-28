using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class QuizQuestions
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; } 
        public int Mark { get; set; }
        public string Explanation { get; set; }
        public int SortOrder { get; set; }

        // Navigation property
        public Quzzes Quiz { get; set; }
        public List<QuizOptions> Answers { get; set; }

        
    }
}