using LMSApi.DALLibrary.Contexts;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LMSApi.DALLibrary.Repositories
{
    public class CourseRepository : AbstractRepository<int, Courses>, ICourseRepository
    {
        public CourseRepository(LMSDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Courses>> GetCoursesByInstructorAsync(int instructorId)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Include(c => c.Enrollments)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Courses> Courses, int TotalCount)> GetCoursesByInstructorPagedAsync(
            int instructorId, LMSApi.ModelLibrary.DTOs.CourseSearchQuery query)
        {
            var queryable = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Where(c => c.InstructorId == instructorId)
                .AsQueryable();

            // 1. Categories
            if (!string.IsNullOrWhiteSpace(query.CategoryIds))
            {
                var catIds = query.CategoryIds.Split(',').Select(int.Parse).ToList();
                queryable = queryable.Where(c => catIds.Contains(c.CategoryId));
            }

            // 2. Levels
            if (!string.IsNullOrWhiteSpace(query.Levels))
            {
                var lvlVals = query.Levels.Split(',').Select(s => (CourseLevel)int.Parse(s)).ToList();
                queryable = queryable.Where(c => lvlVals.Contains(c.Level));
            }

            // 3. Languages
            if (!string.IsNullOrWhiteSpace(query.LanguageIds))
            {
                var langIds = query.LanguageIds.Split(',').Select(int.Parse).ToList();
                queryable = queryable.Where(c => langIds.Contains(c.LanguageId));
            }

            // 4. Statuses
            if (!string.IsNullOrWhiteSpace(query.Statuses))
            {
                var statusVals = query.Statuses.Split(',').Select(s => (CourseStatus)int.Parse(s)).ToList();
                queryable = queryable.Where(c => statusVals.Contains(c.Status));
            }

            // 5. Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                queryable = queryable.Where(c => c.Title.ToLower().Contains(s) || 
                                                (c.Description != null && c.Description.ToLower().Contains(s)));
            }

            var totalCount = await queryable.CountAsync();

            // 6. Sorting
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var sort = query.SortBy.Trim().ToLower();
                queryable = sort switch
                {
                    "enrolled" or "popular" => queryable.OrderByDescending(c => c.Enrollments.Count),
                    "rating" => queryable.OrderByDescending(c => c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0),
                    "newest" => queryable.OrderByDescending(c => c.CreatedAt),
                    "oldest" => queryable.OrderBy(c => c.CreatedAt),
                    _ => queryable.OrderByDescending(c => c.CreatedAt)
                };
            }
            else
            {
                queryable = queryable.OrderByDescending(c => c.CreatedAt);
            }

            var projected = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new 
                {
                    Course = c,
                    EnrolledCount = c.Enrollments.Count()
                })
                .ToListAsync();

            var courses = projected.Select(p => {
                p.Course.ProjectedEnrolledCount = p.EnrolledCount;
                return p.Course;
            }).ToList();

            return (courses, totalCount);
        }

        public async Task<IEnumerable<Courses>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Courses>> GetPublishedCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Where(c => c.Status == CourseStatus.Published)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Courses> Courses, int TotalCount)> GetPublishedCoursesPagedAsync(
            LMSApi.ModelLibrary.DTOs.CourseSearchQuery query)
        {
            var queryable = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.UserProfile)
                .Where(c => c.Status == CourseStatus.Published)
                .AsQueryable();

            // 1. Categories (Multi-select)
            if (!string.IsNullOrWhiteSpace(query.CategoryIds))
            {
                var catIds = query.CategoryIds.Split(',').Select(int.Parse).ToList();
                queryable = queryable.Where(c => catIds.Contains(c.CategoryId));
            }

            // 2. Levels (Multi-select)
            if (!string.IsNullOrWhiteSpace(query.Levels))
            {
                var lvlVals = query.Levels.Split(',').Select(s => (CourseLevel)int.Parse(s)).ToList();
                queryable = queryable.Where(c => lvlVals.Contains(c.Level));
            }

            // 3. Languages (Multi-select)
            if (!string.IsNullOrWhiteSpace(query.LanguageIds))
            {
                var langIds = query.LanguageIds.Split(',').Select(int.Parse).ToList();
                queryable = queryable.Where(c => langIds.Contains(c.LanguageId));
            }

            // 4. Price (Free / Premium)
            if (query.IsPremium.HasValue)
            {
                queryable = queryable.Where(c => c.IsPremium == query.IsPremium.Value);
            }

            // 5. MinRating
            if (query.MinRating.HasValue)
            {
                queryable = queryable.Where(c => c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) >= query.MinRating.Value : 0 >= query.MinRating.Value);
            }

            // 6. Durations (Multi-select)
            if (!string.IsNullOrWhiteSpace(query.Durations))
            {
                var rangeKeys = query.Durations.Split(',').ToList();
                var oneHour = TimeSpan.FromHours(1);
                var fiveHours = TimeSpan.FromHours(5);
                var tenHours = TimeSpan.FromHours(10);
                var twentyHours = TimeSpan.FromHours(20);

                queryable = queryable.Where(c => 
                    (rangeKeys.Contains("lt1") && c.EstimatedDuration < oneHour) ||
                    (rangeKeys.Contains("1to5") && c.EstimatedDuration >= oneHour && c.EstimatedDuration <= fiveHours) ||
                    (rangeKeys.Contains("5to10") && c.EstimatedDuration >= fiveHours && c.EstimatedDuration <= tenHours) ||
                    (rangeKeys.Contains("10to20") && c.EstimatedDuration >= tenHours && c.EstimatedDuration <= twentyHours) ||
                    (rangeKeys.Contains("gt20") && c.EstimatedDuration > twentyHours)
                );
            }

            // 7. Instructors (Multi-select)
            if (!string.IsNullOrWhiteSpace(query.InstructorIds))
            {
                var instIds = query.InstructorIds.Split(',').Select(int.Parse).ToList();
                queryable = queryable.Where(c => instIds.Contains(c.InstructorId));
            }

            // 8. Course Access Types (Multi-select)
            if (!string.IsNullOrWhiteSpace(query.CourseAccessTypes))
            {
                var types = query.CourseAccessTypes.Split(',').Select(s => (CourseAccessType)int.Parse(s)).ToList();
                queryable = queryable.Where(c => types.Contains(c.CourseAccessType));
            }

            // 9. Search query (title, description, or instructor name)
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                queryable = queryable.Where(c => c.Title.ToLower().Contains(s) || 
                                                (c.Description != null && c.Description.ToLower().Contains(s)) ||
                                                (c.Instructor != null && c.Instructor.UserProfile != null && 
                                                 (c.Instructor.UserProfile.FirstName.ToLower() + " " + c.Instructor.UserProfile.LastName.ToLower()).Contains(s)));
            }

            // 11. Exclude course IDs (for enrolled courses)
            if (!string.IsNullOrWhiteSpace(query.ExcludeCourseIds))
            {
                var exclIds = query.ExcludeCourseIds.Split(',').Select(int.Parse).ToList();
                queryable = queryable.Where(c => !exclIds.Contains(c.Id));
            }

            var totalCount = await queryable.CountAsync();

            // 10. Sorting
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var sort = query.SortBy.Trim().ToLower();
                queryable = sort switch
                {
                    "popular" or "trending" => queryable.OrderByDescending(c => c.Enrollments.Count),
                    "rating" => queryable.OrderByDescending(c => c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0),
                    "newest" => queryable.OrderByDescending(c => c.PublishedAt),
                    "oldest" => queryable.OrderBy(c => c.PublishedAt),
                    "az" => queryable.OrderBy(c => c.Title),
                    "za" => queryable.OrderByDescending(c => c.Title),
                    "duration_asc" => queryable.OrderBy(c => c.EstimatedDuration),
                    "duration_desc" => queryable.OrderByDescending(c => c.EstimatedDuration),
                    _ => queryable.OrderByDescending(c => c.PublishedAt)
                };
            }
            else
            {
                queryable = queryable.OrderByDescending(c => c.PublishedAt);
            }

            var projected = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new 
                {
                    Course = c,
                    EnrolledCount = c.Enrollments.Count(),
                    CompletedCount = c.Enrollments.Count(e => e.IsCompleted),
                    LessonsCount = c.Sections.SelectMany(s => s.Lessons).Count()
                })
                .ToListAsync();

            var courses = projected.Select(p => {
                p.Course.ProjectedEnrolledCount = p.EnrolledCount;
                p.Course.ProjectedLessonsCount = p.LessonsCount;
                p.Course.ProjectedCompletionRate =
                p.EnrolledCount > 0
                    ? ((double)p.CompletedCount / p.EnrolledCount) * 100
                    : 0;
                return p.Course;
            }).ToList();

            return (courses, totalCount);
        }

        public async Task<IEnumerable<Courses>> GetPendingCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Where(c => c.Status == CourseStatus.PendingApproval)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Courses> Courses, int TotalCount)> GetPendingCoursesPagedAsync(
            LMSApi.ModelLibrary.DTOs.CourseSearchQuery query)
        {
            var queryable = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.UserProfile)
                .Where(c => c.Status == CourseStatus.PendingApproval)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                queryable = queryable.Where(c => c.Title.ToLower().Contains(s) || 
                                                (c.Description != null && c.Description.ToLower().Contains(s)));
            }

            var totalCount = await queryable.CountAsync();
            queryable = queryable.OrderByDescending(c => c.CreatedAt);

            var projected = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new 
                {
                    Course = c,
                    EnrolledCount = c.Enrollments.Count(),
                    CompletedCount = c.Enrollments.Count(e => e.IsCompleted),
                    LessonsCount = c.Sections.SelectMany(s => s.Lessons).Count()
                })
                .ToListAsync();

            var courses = projected.Select(p => {
                p.Course.ProjectedEnrolledCount = p.EnrolledCount;
                p.Course.ProjectedLessonsCount = p.LessonsCount;
                p.Course.ProjectedCompletionRate = p.EnrolledCount > 0 ? (double)p.CompletedCount / p.EnrolledCount * 100 : 0;
                return p.Course;
            }).ToList();

            return (courses, totalCount);
        }

        public async Task<(IEnumerable<Courses> Courses, int TotalCount)> GetAllCoursesPagedAsync(
            LMSApi.ModelLibrary.DTOs.CourseSearchQuery query)
        {
            var queryable = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.UserProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                queryable = queryable.Where(c => c.Title.ToLower().Contains(s) || 
                                                (c.Description != null && c.Description.ToLower().Contains(s)));
            }
            if (!string.IsNullOrWhiteSpace(query.Statuses))
            {
                var statusVals = query.Statuses.Split(',').Select(s => (CourseStatus)int.Parse(s)).ToList();
                queryable = queryable.Where(c => statusVals.Contains(c.Status));
            }

            var totalCount = await queryable.CountAsync();
            queryable = queryable.OrderByDescending(c => c.CreatedAt);

            var projected = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new 
                {
                    Course = c,
                    EnrolledCount = c.Enrollments.Count(),
                    LessonsCount = c.Sections.SelectMany(s => s.Lessons).Count()
                })
                .ToListAsync();

            var courses = projected.Select(p => {
                p.Course.ProjectedEnrolledCount = p.EnrolledCount;
                p.Course.ProjectedLessonsCount = p.LessonsCount;
                return p.Course;
            }).ToList();

            return (courses, totalCount);
        }

        public async Task<Courses?> GetCourseWithDetailsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.UserProfile)
                .Include(c => c.Enrollments)
                .Include(c => c.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.SortOrder))
                        .ThenInclude(l => l.Resources)
                .Include(c => c.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Quizzes.OrderBy(q => q.Order))
                        .ThenInclude(q => q.Questions)
                .Include(c => c.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Assignments.OrderBy(a => a.SortOrder))
                .Include(c => c.Batches)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Courses?> GetCourseBySlugWithDetailsAsync(string slug)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Language)
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.UserProfile)
                .Include(c => c.Enrollments)
                .Include(c => c.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.SortOrder))
                        .ThenInclude(l => l.Resources)
                .Include(c => c.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Quizzes.OrderBy(q => q.Order))
                        .ThenInclude(q => q.Questions)
                .Include(c => c.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Assignments.OrderBy(a => a.SortOrder))
                .Include(c => c.Batches)
                .FirstOrDefaultAsync(c => c.slug == slug);
        }

        public async Task<LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto> GetCourseRatingStatsAsync(int courseId)
        {
            var stats = await _context.Database
                .SqlQueryRaw<LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto>(
                    "SELECT averagerating AS \"AverageRating\", totalreviews AS \"TotalReviews\" FROM get_course_rating_stats({0})", 
                    courseId)
                .FirstOrDefaultAsync();

            return stats ?? new LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto { AverageRating = 0.0, TotalReviews = 0 };
        }

        public async Task<Dictionary<int, LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto>> GetRatingStatsBatchAsync(IEnumerable<int> courseIds)
        {
            var ids = courseIds.ToList();
            var dict = await _context.Reviews
                .Where(r => ids.Contains(r.CourseId))
                .GroupBy(r => r.CourseId)
                .Select(g => new 
                { 
                    CourseId = g.Key, 
                    Avg = g.Average(r => r.Rating), 
                    Count = g.Count() 
                })
                .ToDictionaryAsync(
                    x => x.CourseId,
                    x => new LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto 
                    { 
                        AverageRating = (double)x.Avg, 
                        TotalReviews = x.Count 
                    }
                );
            return dict;
        }

        public async Task<IEnumerable<CourseLanguages>> GetAllLanguagesAsync()
        {
            return await _context.CourseLanguages.OrderBy(l => l.Name).ToListAsync();
        }

        public async Task<IEnumerable<LMSApi.ModelLibrary.DTOs.InstructorMetadataDto>> GetActiveInstructorsAsync()
        {
            return await _context.Users
                .Where(u => u.RoleId == 2 && u.IsActive) // RoleId 2 is Instructor
                .Include(u => u.UserProfile)
                .Select(u => new LMSApi.ModelLibrary.DTOs.InstructorMetadataDto
                {
                    Id = u.Id,
                    FullName = u.UserProfile != null ? (u.UserProfile.FirstName + " " + u.UserProfile.LastName).Trim() : u.Email
                })
                .OrderBy(i => i.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<LMSApi.ModelLibrary.DTOs.LanguageMetadataDto>> GetActiveLanguagesAsync()
        {
            return await _context.CourseLanguages
                .Select(l => new LMSApi.ModelLibrary.DTOs.LanguageMetadataDto
                {
                    Id = l.Id,
                    Name = l.Name
                })
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task UpdateCourseDurationAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Quizzes)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return;

            var totalCourseDuration = TimeSpan.Zero;
            foreach (var section in course.Sections)
            {
                var sectionDuration = TimeSpan.Zero;
                if (section.Lessons != null)
                {
                    var lessonsDurationTicks = section.Lessons.Sum(l => l.DurationInMinutes?.Ticks ?? 0);
                    sectionDuration += TimeSpan.FromTicks(lessonsDurationTicks);
                }
                if (section.Quizzes != null)
                {
                    var quizzesDurationTicks = section.Quizzes.Sum(q => q.TimeLimit.Ticks);
                    sectionDuration += TimeSpan.FromTicks(quizzesDurationTicks);
                }
                
                section.EstimatedDuration = sectionDuration;
                totalCourseDuration += sectionDuration;
            }

            course.EstimatedDuration = totalCourseDuration;
            await _context.SaveChangesAsync();
        }

        public async Task<LMSApi.ModelLibrary.DTOs.CourseSummaryStatsResponse> GetCourseSummaryStatsAsync()
        {
            var courses = await _context.Courses.ToListAsync();
            return new LMSApi.ModelLibrary.DTOs.CourseSummaryStatsResponse
            {
                TotalCourses = courses.Count,
                PublishedCourses = courses.Count(c => c.Status == CourseStatus.Published),
                PendingApproval = courses.Count(c => c.Status == CourseStatus.PendingApproval),
                ArchivedCourses = courses.Count(c => c.Status == CourseStatus.Archived)
            };
        }

        public async Task SoftDeleteCourseAsync(int courseId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course != null)
            {
                course.IsDeleted = true;
                course.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
