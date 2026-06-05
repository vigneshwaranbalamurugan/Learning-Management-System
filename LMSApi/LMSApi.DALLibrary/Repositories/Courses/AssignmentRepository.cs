using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class AssignmentRepository : AbstractRepository<int, Assignments>, IAssignmentRepository
    {
        public AssignmentRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Assignments>> GetAssignmentsBySectionAsync(int sectionId)
        {
            return await _context.Assignments
                .Where(a => a.CourseSectionId == sectionId)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}
