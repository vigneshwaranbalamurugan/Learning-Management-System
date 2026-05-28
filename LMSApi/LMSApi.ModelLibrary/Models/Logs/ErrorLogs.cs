namespace LMSApi.ModelLibrary.Models
{
    public class ErrorLogs
    {
        public int Id { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set;}
        public DateTime OccurredAt { get; set; }
        public string Source { get; set; } // Optional: to identify where the error occurred (e.g., API endpoint, service name)
    }

}