using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IEnrollmentService
    {
        /// <summary>
        /// Enrolls a student in a course.
        /// For SelfPaced: batchId must be null.
        /// For CohortBased: batchId is required; seats and enrollment window are validated.
        /// </summary>
        Task<EnrollmentResponse> EnrollAsync(int userId, int courseId, int? batchId);

        /// <summary>Returns all enrollments for the calling student.</summary>
        Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(int userId);
    }
}
