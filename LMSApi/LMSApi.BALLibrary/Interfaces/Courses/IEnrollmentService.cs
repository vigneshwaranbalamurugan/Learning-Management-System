using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IEnrollmentService
    {
        
        Task<EnrollmentResponse> EnrollAsync(int userId, int courseId, int? batchId);

        /// <summary>Returns all enrollments for the calling student.</summary>
        Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(int userId);
    }
}
