namespace LMSApi.ModelLibrary.DTOs
{
    // ── Requests ──────────────────────────────────────────────────────────────
    public class CreateCourseOrderRequest
    {
        public int CourseId { get; set; }
        public string Currency { get; set; } = "INR";
    }

    public class ConfirmCoursePaymentRequest
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
        public int CourseId { get; set; }
        /// <summary>Platform fee config ID returned from the order creation step.</summary>
        public int? PlatformFeeConfigId { get; set; }
        public decimal PlatformFeeAmount { get; set; }
        public decimal InstructorAmount { get; set; }
    }

    // ── Responses ─────────────────────────────────────────────────────────────
    public class CourseOrderResponse
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PlatformFeeAmount { get; set; }
        public decimal InstructorAmount { get; set; }
        public string Currency { get; set; } = "INR";
        public string FeeDescription { get; set; } = string.Empty;
        public int? PlatformFeeConfigId { get; set; }
    }

    public class PaymentConfirmResponse
    {
        public int PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal InstructorAmount { get; set; }
        public DateTime PaidAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
