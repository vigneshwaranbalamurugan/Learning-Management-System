using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IAssignmentRepository : IRepository<int, Assignments>
    {
        Task<IEnumerable<Assignments>> GetAssignmentsBySectionAsync(int sectionId);
        Task<IEnumerable<InstructorAssignmentSummaryDto>> GetInstructorAssignmentsAsync(int instructorId);
    }
}
