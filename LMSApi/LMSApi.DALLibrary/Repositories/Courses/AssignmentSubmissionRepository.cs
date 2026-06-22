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
    }
}
