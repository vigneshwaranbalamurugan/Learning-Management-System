using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Payments
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int EnrollmentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? RawResponse { get; set; }

        // Navigation properties
        public Users User { get; set; } = null!;
        public Courses Course { get; set; } = null!;
        public Enrollments Enrollment { get; set; } = null!;
    }
}