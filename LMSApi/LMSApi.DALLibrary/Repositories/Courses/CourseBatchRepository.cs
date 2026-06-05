using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class CourseBatchRepository : AbstractRepository<int, CourseBatch>, ICourseBatchRepository
    {
        public CourseBatchRepository(LMSDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CourseBatch>> GetBatchesByCourseAsync(int courseId)
        {
            var batches = await _context.CourseBatches
                .Where(b => b.CourseId == courseId)
                .OrderBy(b => b.StartDate)
                .ToListAsync();

            // Populate AvailableSeats via the PostgreSQL function for each batch
            foreach (var batch in batches)
            {
                batch.AvailableSeats = await GetAvailableSeatsAsync(batch.Id);
            }

            return batches;
        }

        /// <inheritdoc/>
        public async Task<CourseBatch?> GetActiveBatchAsync(int courseId)
        {
            var batch = await _context.CourseBatches
                .Where(b => b.CourseId == courseId && b.Status == BatchStatus.Active)
                .FirstOrDefaultAsync();

            if (batch != null)
                batch.AvailableSeats = await GetAvailableSeatsAsync(batch.Id);

            return batch;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CourseBatch>> GetUpcomingBatchesAsync(int courseId)
        {
            var batches = await _context.CourseBatches
                .Where(b => b.CourseId == courseId && b.Status == BatchStatus.Upcoming)
                .OrderBy(b => b.StartDate)
                .ToListAsync();

            foreach (var batch in batches)
            {
                batch.AvailableSeats = await GetAvailableSeatsAsync(batch.Id);
            }

            return batches;
        }

        /// <summary>
        /// Calls the PostgreSQL function <c>get_batch_available_seats(p_batch_id)</c>
        /// and returns the computed available seat count.
        /// </summary>
        public async Task<int> GetAvailableSeatsAsync(int batchId)
        {
            // EF Core 7+ SqlQuery<T> for scalar results
            var result = await _context.Database
                .SqlQuery<int>($"SELECT get_batch_available_seats({batchId})")
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
