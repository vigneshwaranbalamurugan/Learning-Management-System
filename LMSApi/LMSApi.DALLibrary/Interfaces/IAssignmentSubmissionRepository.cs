using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IAssignmentSubmissionRepository : IRepository<int, AssignmentSubmissions>
    {
        /// <summary>Returns all submissions by a student for a given assignment (all attempts).</summary>
        Task<IEnumerable<AssignmentSubmissions>> GetStudentSubmissionsAsync(int assignmentId, int studentId);

        /// <summary>Returns all submission attempts in order for a student on an assignment.</summary>
        Task<IEnumerable<AssignmentSubmissions>> GetSubmissionAttemptsAsync(int assignmentId, int studentId);

        /// <summary>Applies grade and feedback to a submission; sets GradedAt and Status to Graded.</summary>
        Task GradeSubmissionAsync(int submissionId, int marksAwarded, string feedback);

        /// <summary>Returns all submissions with status Submitted or UnderReview for instructor review.</summary>
        Task<IEnumerable<AssignmentSubmissions>> GetPendingSubmissionsAsync(int assignmentId);

        /// <summary>Calls PostgreSQL function get_submission_attempt_count.</summary>
        Task<int> GetSubmissionAttemptCountAsync(int assignmentId, int studentId);

        /// <summary>Calls PostgreSQL function calculate_assignment_pass_status.</summary>
        Task<bool> CalculateAssignmentPassStatusAsync(int submissionId);

        /// <summary>Calls PostgreSQL function calculate_assignment_completion.</summary>
        Task<bool> HasStudentPassedMandatoryAssignmentsAsync(int studentId, int sectionId);
    }
}
