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
                .Where(a => a.CourseSection.Course.InstructorId == instructorId && a.CourseSection.Course.Status == LMSApi.ModelLibrary.Enums.CourseStatus.Published)
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

        public async Task<PagedInstructorAssignmentResponse> GetInstructorAssignmentsPagedAsync(int instructorId, int pageNumber, int pageSize, string? searchQuery, int? statusFilter)
        {
            var query = _context.Assignments
                .Include(a => a.CourseSection)
                    .ThenInclude(cs => cs.Course)
                .Where(a => a.CourseSection.Course.InstructorId == instructorId && a.CourseSection.Course.Status == LMSApi.ModelLibrary.Enums.CourseStatus.Published)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(lowerQuery) || 
                                         a.CourseSection.Course.Title.ToLower().Contains(lowerQuery) ||
                                         a.CourseSection.Title.ToLower().Contains(lowerQuery));
            }

            var allAssignments = await query
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

            if (statusFilter.HasValue)
            {
                // statusFilter: 0 = All, 1 = Pending Submissions, 2 = Fully Graded
                if (statusFilter.Value == 1)
                {
                    allAssignments = allAssignments.Where(a => a.PendingSubmissionsCount > 0).ToList();
                }
                else if (statusFilter.Value == 2)
                {
                    allAssignments = allAssignments.Where(a => a.PendingSubmissionsCount == 0).ToList();
                }
            }

            var totalCount = allAssignments.Count;
            var pendingCount = allAssignments.Count(a => a.PendingSubmissionsCount > 0);
            var fullyGradedCount = allAssignments.Count(a => a.PendingSubmissionsCount == 0);
            var uniqueCourseCount = allAssignments.Select(a => a.CourseTitle).Distinct().Count();

            var pagedAssignments = allAssignments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedInstructorAssignmentResponse
            {
                Assignments = pagedAssignments,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                TotalPendingCount = pendingCount,
                FullyGradedCount = fullyGradedCount,
                UniqueCourseCount = uniqueCourseCount
            };
        }

        public async Task<PagedLearnerAssignmentResponse> GetLearnerAssignmentsAsync(int userId, int pageNumber, int pageSize, string? searchQuery = null)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId && e.EnrollmentStatus == LMSApi.ModelLibrary.Enums.EnrollmentStatus.Active)
                .ToListAsync();
            var enrolledCourseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();
            var courseEnrollmentDates = enrollments.ToDictionary(e => e.CourseId, e => e.EnrolledAt);

            var query = _context.Assignments
                .Include(a => a.CourseSection)
                    .ThenInclude(cs => cs.Course)
                .Where(a => enrolledCourseIds.Contains(a.CourseSection.CourseId) && a.Status == LMSApi.ModelLibrary.Enums.PublishStatus.Published);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(lowerQuery) || 
                                         a.CourseSection.Course.Title.ToLower().Contains(lowerQuery) ||
                                         a.CourseSection.Title.ToLower().Contains(lowerQuery));
            }

            var totalCount = await query.CountAsync();
            
            var assignmentsList = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var assignmentIds = assignmentsList.Select(a => a.Id).ToList();
            
            var submissions = await _context.AssignmentSubmissions
                .Where(s => s.StudentId == userId && assignmentIds.Contains(s.AssignmentId))
                .ToListAsync();

            // Calculate global stats
            var allUserSubmissions = await _context.AssignmentSubmissions
                .Where(s => s.StudentId == userId)
                .ToListAsync();

            int passedCount = 0;
            int failedCount = 0;
            
            var allQuery = _context.Assignments.Where(a => enrolledCourseIds.Contains(a.CourseSection.CourseId) && a.Status == LMSApi.ModelLibrary.Enums.PublishStatus.Published);
            var allAssignmentsIds = await allQuery.Select(a => a.Id).ToListAsync();
            var allAssignmentsPassingMarks = await allQuery.ToDictionaryAsync(a => a.Id, a => a.PassingMarks);

            var groupedAllSubmissions = allUserSubmissions
                .Where(s => allAssignmentsIds.Contains(s.AssignmentId))
                .GroupBy(s => s.AssignmentId)
                .ToList();

            int submittedCount = 0;
            foreach(var grp in groupedAllSubmissions)
            {
                var latest = grp.OrderByDescending(s => s.SubmittedAt).FirstOrDefault();
                if(latest != null)
                {
                    submittedCount++;
                    if(latest.Status == LMSApi.ModelLibrary.Enums.SubmissionStatus.Graded)
                    {
                        if(allAssignmentsPassingMarks.TryGetValue(grp.Key, out int pm) && latest.MarksAwarded >= pm)
                            passedCount++;
                        else
                            failedCount++;
                    }
                }
            }
            int pendingCount = totalCount - passedCount - failedCount; // This includes Not Submitted + Submitted + UnderReview

            var dtos = assignmentsList.Select(a => {
                var aSubs = submissions.Where(s => s.AssignmentId == a.Id).OrderByDescending(s => s.SubmittedAt).ToList();
                var attemptsMade = aSubs.Count;
                var remainingAttempts = Math.Max(0, a.MaxSubmissions - attemptsMade);
                var latestSub = aSubs.FirstOrDefault();
                bool? isPassed = null;
                
                string latestStatus = "Pending";
                if (latestSub != null)
                {
                    latestStatus = latestSub.Status.ToString();
                    if (latestSub.Status == LMSApi.ModelLibrary.Enums.SubmissionStatus.Graded) 
                    {
                         isPassed = latestSub.MarksAwarded >= a.PassingMarks;
                    }
                }

                DateTime? calculatedDeadline = a.DeadlineDate;
                if (calculatedDeadline == null && a.DeadlineInDays > 0)
                {
                    if (courseEnrollmentDates.TryGetValue(a.CourseSection.CourseId, out var enrolledAt))
                    {
                        calculatedDeadline = enrolledAt.AddDays(a.DeadlineInDays);
                    }
                }

                return new LearnerAssignmentDto
                {
                    Id = a.Id,
                    CourseSectionId = a.CourseSectionId,
                    Title = a.Title,
                    Description = a.Description,
                    Instructions = a.Instructions,
                    IsCompulsory = a.IsCompulsory,
                    TotalMarks = a.TotalMarks,
                    PassingMarks = a.PassingMarks,
                    AttachmentType = a.AttachmentType,
                    AttachmentUrl = a.AttachmentUrl,
                    DeadlineInDays = a.DeadlineInDays,
                    DeadlineDate = calculatedDeadline,
                    MaxSubmissions = a.MaxSubmissions,
                    IsLateSubmissionAllowed = a.IsLateSubmissionAllowed,
                    SortOrder = a.SortOrder,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    CourseTitle = a.CourseSection.Course.Title,
                    SectionTitle = a.CourseSection.Title,
                    AttemptsMade = attemptsMade,
                    RemainingAttempts = remainingAttempts,
                    IsPassed = isPassed,
                    LatestStatus = latestStatus
                };
            }).ToList();

            return new PagedLearnerAssignmentResponse
            {
                Assignments = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PendingCount = pendingCount,
                PassedCount = passedCount,
                FailedCount = failedCount
            };
        }
    }
}
