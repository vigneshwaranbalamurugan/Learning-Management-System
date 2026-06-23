using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.DTOs;
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
                .OrderBy(a => a.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<InstructorAssignmentSummaryDto>> GetInstructorAssignmentsAsync(int instructorId)
        {
            return await _context.Assignments
                .Include(a => a.CourseSection)
                    .ThenInclude(cs => cs.Course)
                .Where(a => a.CourseSection.Course.InstructorId == instructorId)
                .Select(a => new InstructorAssignmentSummaryDto
                {
                    Id = a.Id,
                    CourseSectionId = a.CourseSectionId,
                    Title = a.Title,
                    CourseTitle = a.CourseSection.Course.Title,
                    SectionTitle = a.CourseSection.Title,
                    TotalMarks = a.TotalMarks,
                    DeadlineInDays = a.DeadlineInDays,
                    DeadlineDate = a.DeadlineDate,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    PendingSubmissionsCount = _context.AssignmentSubmissions
                        .Count(s => s.AssignmentId == a.Id && (s.Status == LMSApi.ModelLibrary.Enums.SubmissionStatus.Submitted || s.Status == LMSApi.ModelLibrary.Enums.SubmissionStatus.UnderReview))
                })
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}
