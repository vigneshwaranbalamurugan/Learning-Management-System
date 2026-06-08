using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IAssignmentService
    {
        // ─── Assignment CRUD ────────────────────────────────────────────────
        Task<IEnumerable<AssignmentResponse>> GetAssignmentsBySectionAsync(int sectionId);
        Task<AssignmentResponse> GetAssignmentByIdAsync(int id);
        Task<AssignmentResponse> CreateAssignmentAsync(CreateAssignmentRequest request, Stream? attachmentStream = null, string? attachmentFileName = null);
        Task<AssignmentResponse> UpdateAssignmentAsync(int id, UpdateAssignmentRequest request, Stream? attachmentStream = null, string? attachmentFileName = null);
        Task DeleteAssignmentAsync(int id);
        Task<AssignmentResponse> PublishAssignmentAsync(int id, bool publish);

        // ─── Submission Workflow ────────────────────────────────────────────
        Task<AssignmentSubmissionResponse> SubmitAssignmentAsync(int studentId, AssignmentSubmissionRequest request, Stream? attachmentStream = null, string? attachmentFileName = null);
        Task<AssignmentSubmissionResponse> GradeAssignmentAsync(int submissionId, GradeSubmissionRequest request);

        // ─── Queries ────────────────────────────────────────────────────────
        Task<AssignmentStatusResponse> GetStudentAssignmentStatusAsync(int assignmentId, int studentId);
        Task<IEnumerable<AssignmentSubmissionResponse>> GetPendingReviewsAsync(int assignmentId);
        Task<IEnumerable<AssignmentSubmissionResponse>> GetStudentSubmissionsAsync(int assignmentId, int studentId);
    }
}
