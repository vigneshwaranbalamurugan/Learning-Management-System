using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IEnrollmentService
    {
        
        Task<EnrollmentResponse> EnrollInFreeCourseAsync(int userId, int courseId, int? batchId);
        Task<string> EnrollInPremiumCourseAsync(int userId, int courseId, int? batchId);
        Task<EnrollmentResponse> VerifyPaymentAndEnrollAsync(int userId, int courseId, int? batchId, string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
        
        Task<DateTime?> CalculateAssignmentDeadlineAsync(int userId, int assignmentId);
        Task<bool> ValidateCourseAccessAsync(int enrollmentId);
        Task<bool> ValidateBatchEnrollmentAsync(int batchId);

        /// <summary>Returns all enrollments for the calling student.</summary>
        Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(int userId);
    }
}
