using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IAssignmentSubmissionService
    {
        // ─── Submission Workflow ────────────────────────────────────────────
        Task<AssignmentSubmissionResponse> SubmitAssignmentAsync(int studentId, AssignmentSubmissionRequest request, Stream? attachmentStream = null, string? attachmentFileName = null);
        Task<AssignmentSubmissionResponse> GradeAssignmentAsync(int submissionId, GradeSubmissionRequest request);
       
        // ─── Queries ────────────────────────────────────────────────────────
        Task<AssignmentStatusResponse> GetStudentAssignmentStatusAsync(int assignmentId, int studentId);
        Task<IEnumerable<AssignmentSubmissionResponse>> GetPendingReviewsAsync(int assignmentId);
        Task<IEnumerable<AssignmentSubmissionResponse>> GetGradedReviewsAsync(int assignmentId);
        Task<IEnumerable<AssignmentSubmissionResponse>> GetStudentSubmissionsAsync(int assignmentId, int studentId);

    }
}