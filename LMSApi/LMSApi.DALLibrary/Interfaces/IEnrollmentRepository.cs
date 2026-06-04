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

        /// <summary>Checks if a course has any enrollments.</summary>
        Task<bool> HasEnrollmentsByCourseAsync(int courseId);
    }
}
