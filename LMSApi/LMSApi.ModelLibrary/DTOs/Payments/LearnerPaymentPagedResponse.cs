using System.Collections.Generic;

namespace LMSApi.ModelLibrary.DTOs
{
    public record LearnerPaymentPagedResponse
    {
        public IEnumerable<LearnerPaymentResponse> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public int CurrentPage { get; init; }
    }
}
