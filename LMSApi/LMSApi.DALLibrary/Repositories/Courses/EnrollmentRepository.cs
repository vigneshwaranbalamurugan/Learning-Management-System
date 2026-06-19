using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class EnrollmentRepository : AbstractRepository<int, Enrollments>, IEnrollmentRepository
    {
        public EnrollmentRepository(LMSDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Enrollments?> GetByUserAndCourseAsync(int userId, int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Batch)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Enrollments>> GetEnrollmentsByBatchAsync(int batchId)
        {
            return await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.BatchId == batchId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Enrollments>> GetEnrollmentsByUserAsync(int userId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Batch)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Enrollments>> GetActiveEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(e => e.Batch)
                .Where(e => e.CourseId == courseId && e.EnrollmentStatus == LMSApi.ModelLibrary.Enums.EnrollmentStatus.Active)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> HasEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments.AnyAsync(e => e.CourseId == courseId);
        }

        public async Task<bool> IsAlreadyEnrolledAsync(int userId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && e.EnrollmentStatus == LMSApi.ModelLibrary.Enums.EnrollmentStatus.Active);
        }

        public async Task<Enrollments?> GetActiveEnrollmentAsync(int userId, int courseId)
        {
            return await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId && e.EnrollmentStatus == LMSApi.ModelLibrary.Enums.EnrollmentStatus.Active);
        }

        public async Task<int> GetAvailableSeatsAsync(int batchId)
        {
            // Will call Postgres function `calculate_available_seats`
            // For now, EF Core raw SQL mapping or manual calculation
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT calculate_available_seats(@batchId)";
                var param = command.CreateParameter();
                param.ParameterName = "@batchId";
                param.Value = batchId;
                command.Parameters.Add(param);
                
                await _context.Database.OpenConnectionAsync();
                var result = await command.ExecuteScalarAsync();
                return result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<Enrollments> CreateEnrollmentAsync(Enrollments enrollment)
        {
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<bool> ValidateBatchEnrollmentAsync(int batchId)
        {
            var batch = await _context.CourseBatches.FindAsync(batchId);
            if (batch == null) return false;
            
            if (DateTime.UtcNow < batch.EnrollmentStartDate || DateTime.UtcNow > batch.EnrollmentEndDate)
                return false;

            var availableSeats = await GetAvailableSeatsAsync(batchId);
            return availableSeats > 0;
        }

        public async Task<DateTime?> GetCourseAccessAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            return enrollment?.AccessExpiresAt;
        }
    }
}
