using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IBatchService
    {
        /// <summary>Creates a new batch for a CohortBased course.</summary>
        Task<BatchResponse> CreateBatchAsync(int courseId, CreateBatchRequest request);

        /// <summary>Updates an existing batch. Only provided fields are changed.</summary>
        Task<BatchResponse> UpdateBatchAsync(int batchId, UpdateBatchRequest request);

        /// <summary>Deletes a batch. Throws if the batch has active enrollments.</summary>
        Task DeleteBatchAsync(int batchId);

        /// <summary>Returns a specific batch by its ID.</summary>
        Task<BatchResponse> GetBatchByIdAsync(int batchId);

        /// <summary>Returns all batches for a course including computed AvailableSeats.</summary>
        Task<IEnumerable<BatchResponse>> GetBatchesByCourseAsync(int courseId);
    }
}
