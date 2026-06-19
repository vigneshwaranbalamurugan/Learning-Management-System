using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ICourseBatchRepository : IRepository<int, CourseBatch>
    {
        /// <summary>Returns all batches for a given course, with AvailableSeats populated via PostgreSQL function.</summary>
        Task<IEnumerable<CourseBatch>> GetBatchesByCourseAsync(int courseId);

        /// <summary>Returns the single active batch for a course, or null.</summary>
        Task<CourseBatch?> GetActiveBatchAsync(int courseId);

        /// <summary>Returns all upcoming (not yet started) batches for a course.</summary>
        Task<IEnumerable<CourseBatch>> GetUpcomingBatchesAsync(int courseId);

        /// <summary>
        /// Calls the PostgreSQL function <c>get_batch_available_seats(batch_id)</c>
        /// and returns the result directly from the database.
        /// </summary>
        Task<int> GetAvailableSeatsAsync(int batchId);
    }
}
