using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IAssignmentRepository : IRepository<int, Assignments>
    {
        Task<IEnumerable<Assignments>> GetAssignmentsBySectionAsync(int sectionId);
    }
}
