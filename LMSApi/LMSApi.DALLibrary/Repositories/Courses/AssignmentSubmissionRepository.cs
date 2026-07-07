using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class AssignmentSubmissionRepository : AbstractRepository<int, AssignmentSubmissions>, IAssignmentSubmissionRepository
    {
        public AssignmentSubmissionRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AssignmentSubmissions>> GetStudentSubmissionsAsync(int assignmentId, int studentId)
        {
            return await _context.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId && s.StudentId == studentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentSubmissions>> GetSubmissionAttemptsAsync(int assignmentId, int studentId)
        {
            return await _context.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId && s.StudentId == studentId)
                .OrderBy(s => s.AttemptNumber)
                .ToListAsync();
        }

        public async Task GradeSubmissionAsync(int submissionId, int marksAwarded, string feedback)
        {
            var submission = await GetByIdAsync(submissionId);

            submission.MarksAwarded = marksAwarded;
            submission.Feedback = feedback;
            submission.GradedAt = DateTime.UtcNow;
            submission.Status = SubmissionStatus.Graded;

            await UpdateAsync(submission);
        }

        public async Task<IEnumerable<AssignmentSubmissions>> GetPendingSubmissionsAsync(int assignmentId)
        {
            return await _context.AssignmentSubmissions
                .Include(s => s.Student)
                    .ThenInclude(u => u.UserProfile)
                .Where(s => s.AssignmentId == assignmentId &&
                            (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview))
                .OrderBy(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentSubmissions>> GetGradedSubmissionsAsync(int assignmentId)
        {
            return await _context.AssignmentSubmissions
                .Include(s => s.Student)
                    .ThenInclude(u => u.UserProfile)
                .Where(s => s.AssignmentId == assignmentId && s.Status == SubmissionStatus.Graded)
                .OrderByDescending(s => s.GradedAt)
                .ToListAsync();
        }

        public async Task<int> GetSubmissionAttemptCountAsync(int assignmentId, int studentId)
        {
            return await _context.Database
                .SqlQuery<int>($"SELECT get_submission_attempt_count({assignmentId}, {studentId}) AS \"Value\"")
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CalculateAssignmentPassStatusAsync(int submissionId)
        {
            return await _context.Database
                .SqlQuery<bool>($"SELECT calculate_assignment_pass_status({submissionId}) AS \"Value\"")
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasStudentPassedMandatoryAssignmentsAsync(int studentId, int sectionId)
        {
            return await _context.Database
                .SqlQuery<bool>($"SELECT calculate_assignment_completion({studentId}, {sectionId}) AS \"Value\"")
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetPassedAssignmentsCountAsync(int studentId, List<int> assignmentIds)
        {
            if (assignmentIds == null || !assignmentIds.Any()) return 0;

            return await _context.AssignmentSubmissions
                .Where(s => s.StudentId == studentId && assignmentIds.Contains(s.AssignmentId) && s.IsPassed == true)
                .Select(s => s.AssignmentId)
                .Distinct()
                .CountAsync();
        }

        public async Task<IEnumerable<AssignmentSubmissions>> GetSubmissionsForAssignmentsAsync(int studentId, List<int> assignmentIds)
        {
            if (assignmentIds == null || !assignmentIds.Any()) return new List<AssignmentSubmissions>();

            return await _context.AssignmentSubmissions
                .Where(s => s.StudentId == studentId && assignmentIds.Contains(s.AssignmentId))
                .ToListAsync();
        }

        public async Task<(IEnumerable<AssignmentSubmissions> Submissions, int TotalCount)> GetAllSubmissionsPagedAsync(int pageNumber, int pageSize, string? status, string? search = null)
        {
            var queryable = _context.AssignmentSubmissions
                .Include(s => s.Student)
                    .ThenInclude(u => u.UserProfile)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.CourseSection)
                        .ThenInclude(cs => cs.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SubmissionStatus>(status, true, out var parsedStatus))
            {
                queryable = queryable.Where(s => s.Status == parsedStatus);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                queryable = queryable.Where(s => 
                    s.Assignment.CourseSection.Course.Title.ToLower().Contains(searchLower) ||
                    s.Student.UserProfile.FirstName.ToLower().Contains(searchLower) ||
                    s.Student.UserProfile.LastName.ToLower().Contains(searchLower) ||
                    s.Student.Email.ToLower().Contains(searchLower)
                );
            }

            var totalCount = await queryable.CountAsync();
            
            var submissions = await queryable
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (submissions, totalCount);
        }
    }
}
