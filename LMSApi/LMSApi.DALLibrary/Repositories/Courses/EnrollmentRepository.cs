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
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Language)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.UserProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                .Include(e => e.Batch)
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
                .Include(e => e.Course)
                    .ThenInclude(c => c.Language)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.UserProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                .Include(e => e.Batch)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Enrollments> Enrollments, int TotalCount)> GetEnrollmentsByUserPagedAsync(int userId, int pageNumber, int pageSize, string? search = null, string? status = null, string? accessType = null)
        {
            var query = _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Language)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.UserProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                .Include(e => e.Batch)
                .Where(e => e.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(e => e.Course.Title.ToLower().Contains(lowerSearch) || (e.Course.Instructor.UserProfile != null && e.Course.Instructor.UserProfile.FirstName.ToLower().Contains(lowerSearch)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("completed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.IsCompleted);
                }
                else if (status.Equals("in_progress", StringComparison.OrdinalIgnoreCase) || status.Equals("in-progress", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => !e.IsCompleted);
                }
            }

            if (!string.IsNullOrWhiteSpace(accessType))
            {
                if (Enum.TryParse<LMSApi.ModelLibrary.Enums.CourseAccessType>(accessType, true, out var parsedAccessType))
                {
                    query = query.Where(e => e.Course.CourseAccessType == parsedAccessType);
                }
            }

            var totalCount = await query.CountAsync();

            var enrollments = await query
                .OrderByDescending(e => e.EnrolledAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (enrollments, totalCount);
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

        /// <inheritdoc/>
        public async Task<bool> HasNonExpiredEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments.AnyAsync(e =>
                e.CourseId == courseId &&
                (e.EnrollmentStatus == LMSApi.ModelLibrary.Enums.EnrollmentStatus.Active || e.EnrollmentStatus == LMSApi.ModelLibrary.Enums.EnrollmentStatus.Completed));
        }

        /// <inheritdoc/>
        public async Task<bool> HasActiveOnlyEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments.AnyAsync(e =>
                e.CourseId == courseId && e.EnrollmentStatus == LMSApi.ModelLibrary.Enums.EnrollmentStatus.Active);
        }

        public async Task<bool> IsAlreadyEnrolledAsync(int userId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && e.EnrollmentStatus == LMSApi.ModelLibrary.Enums.EnrollmentStatus.Active);
        }

        public async Task<Enrollments?> GetActiveEnrollmentAsync(int userId, int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Language)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.UserProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                .Include(e => e.Batch)
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

        public async Task<IEnumerable<Enrollments>> GetEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(e => e.Batch)
                .Where(e => e.CourseId == courseId)
                .OrderBy(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task SetIsOnLatestVersionForCourseAsync(int courseId, bool value)
        {
            await _context.Enrollments
                .Where(e => e.CourseId == courseId && e.EnrollmentStatus == LMSApi.ModelLibrary.Enums.EnrollmentStatus.Active)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsOnLatestVersion, value));
        }
    }
}
