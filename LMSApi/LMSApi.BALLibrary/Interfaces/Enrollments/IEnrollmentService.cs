using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IEnrollmentService
    {
        
        Task<EnrollmentResponse> EnrollInFreeCourseAsync(int userId, int courseId, int? batchId);
        Task<string> EnrollInPremiumCourseAsync(int userId, int courseId, int? batchId, string providerName);
        Task<EnrollmentResponse> VerifyPaymentAndEnrollAsync(int userId, int courseId, VerifyPaymentRequest request);
        Task ProcessWebhookPaymentAsync(string providerOrderId, string providerPaymentId, string eventType, string? rawResponse = null);
        
        Task<DateTime?> CalculateAssignmentDeadlineAsync(int userId, int assignmentId);
        Task<bool> ValidateCourseAccessAsync(int enrollmentId);
        Task<bool> ValidateBatchEnrollmentAsync(int batchId);

        /// <summary>Returns all enrollments for the calling student.</summary>
        Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(int userId);

        /// <summary>Returns paginated enrollments for the calling student.</summary>
        Task<EnrollmentResponse> UpdateToLatestVersionAsync(int userId, int courseId);
        Task<PagedEnrollmentResponse> GetMyEnrollmentsPagedAsync(int studentId, int pageNumber = 1, int pageSize = 10, string? search = null, string? status = null, string? accessType = null);
    }
}
