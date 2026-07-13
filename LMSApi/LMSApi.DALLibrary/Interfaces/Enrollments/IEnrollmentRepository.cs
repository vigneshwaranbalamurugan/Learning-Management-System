using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IEnrollmentRepository : IRepository<int, Enrollments>
    {
        /// <summary>Returns the enrollment for a specific user + course combination, or null.</summary>
        Task<Enrollments?> GetByUserAndCourseAsync(int userId, int courseId);

        /// <summary>Returns all enrollments belonging to a specific batch.</summary>
        Task<IEnumerable<Enrollments>> GetEnrollmentsByBatchAsync(int batchId);

        /// <summary>Returns all enrollments for a given user, including course and batch navigation.</summary>
        Task<IEnumerable<Enrollments>> GetEnrollmentsByUserAsync(int userId);

        /// <summary>Returns paginated enrollments for a given user.</summary>
        Task<(IEnumerable<Enrollments> Enrollments, int TotalCount)> GetEnrollmentsByUserPagedAsync(int userId, int pageNumber, int pageSize, string? search = null, string? status = null, string? accessType = null);

        /// <summary>Returns all active enrollments for a given course.</summary>
        Task<IEnumerable<Enrollments>> GetActiveEnrollmentsByCourseAsync(int courseId);

        Task SetIsOnLatestVersionForCourseAsync(int courseId, bool value);

        /// <summary>Checks if a course has any enrollments.</summary>
        Task<bool> HasEnrollmentsByCourseAsync(int courseId);

        /// <summary>Returns true if the course has any Active or Completed enrollments.</summary>
        Task<bool> HasNonExpiredEnrollmentsByCourseAsync(int courseId);

        /// <summary>Returns true if the course has any Active-only enrollments.</summary>
        Task<bool> HasActiveOnlyEnrollmentsByCourseAsync(int courseId);

        Task<bool> IsAlreadyEnrolledAsync(int userId, int courseId);
        Task<Enrollments?> GetActiveEnrollmentAsync(int userId, int courseId);
        Task<int> GetAvailableSeatsAsync(int batchId);
        Task<Enrollments> CreateEnrollmentAsync(Enrollments enrollment);
        Task<bool> ValidateBatchEnrollmentAsync(int batchId);
        Task<DateTime?> GetCourseAccessAsync(int enrollmentId);
        Task<IEnumerable<Enrollments>> GetEnrollmentsByCourseAsync(int courseId);
    }
}
