using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IAssignmentRepository : IRepository<int, Assignments>
    {
        Task<IEnumerable<Assignments>> GetAssignmentsBySectionAsync(int sectionId);
        Task<IEnumerable<InstructorAssignmentSummaryDto>> GetInstructorAssignmentsAsync(int instructorId);
        Task<PagedLearnerAssignmentResponse> GetLearnerAssignmentsAsync(int userId, int pageNumber, int pageSize, string? searchQuery = null);
    }
}
