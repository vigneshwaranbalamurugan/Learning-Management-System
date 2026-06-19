using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class CreateBatchRequest
    {
        [Required(ErrorMessage = "Batch name is required.")]
        [MaxLength(200, ErrorMessage = "Batch name must not exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Enrollment start date is required.")]
        public DateTime EnrollmentStartDate { get; set; }

        [Required(ErrorMessage = "Enrollment end date is required.")]
        public DateTime EnrollmentEndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxStudents must be at least 1.")]
        public int MaxStudents { get; set; }
    }

    public class UpdateBatchRequest
    {
        [MaxLength(200, ErrorMessage = "Batch name must not exceed 200 characters.")]
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EnrollmentStartDate { get; set; }
        public DateTime? EnrollmentEndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxStudents must be at least 1.")]
        public int? MaxStudents { get; set; }

        public BatchStatus? Status { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class BatchResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime EnrollmentStartDate { get; set; }
        public DateTime EnrollmentEndDate { get; set; }
        public int MaxStudents { get; set; }
        public BatchStatus Status { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Lightweight batch summary returned inside <see cref="CourseDetailsResponse.AvailableBatches"/>.
    /// </summary>
    public class BatchSummaryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BatchStatus Status { get; set; }
        public int MaxStudents { get; set; }
        public int AvailableSeats { get; set; }
    }
}
