using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Payments
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } // e.g., Credit Card, PayPal
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; }
        public PaymentStatus Status { get; set; }

        // Navigation properties
        public Users User { get; set; }
        public Courses Course { get; set; }
    }
}