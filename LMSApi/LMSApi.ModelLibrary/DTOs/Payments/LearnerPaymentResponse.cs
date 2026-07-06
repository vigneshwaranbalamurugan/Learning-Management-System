using System;

namespace LMSApi.ModelLibrary.DTOs
{
    public record LearnerPaymentResponse
    {
        public int Id { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public string? CourseThumbnailUrl { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "INR";
        public string Status { get; init; } = string.Empty;
        public DateTime? PaidAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? ProviderPaymentId { get; init; }
        public string InvoiceNumber => $"INV-{Id}";
    }
}
