using AutoMapper;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Mappers
{
    public class CourseMapperProfile : Profile
    {
        public CourseMapperProfile()
        {
            // ─── Category ────────────────────────────────────────────────────
            CreateMap<CourseCategories, CategoryResponse>();
            CreateMap<CreateCategoryRequest, CourseCategories>();
            CreateMap<UpdateCategoryRequest, CourseCategories>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ─── Course ────────────────────────────────────────────────────
            CreateMap<Courses, CourseResponse>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.slug))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.LanguageName, opt => opt.MapFrom(src => src.Language != null ? src.Language.Name : string.Empty))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null && src.Instructor.UserProfile != null ? $"{src.Instructor.UserProfile.FirstName} {src.Instructor.UserProfile.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.LessonsCount, opt => opt.MapFrom(src => src.Sections != null ? src.Sections.SelectMany(s => s.Lessons).Count() : 0))
                .ForMember(dest => dest.HasCertificate, opt => opt.MapFrom(src => true));

            CreateMap<Courses, CourseDetailsResponse>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.slug))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.LanguageName, opt => opt.MapFrom(src => src.Language != null ? src.Language.Name : string.Empty))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null && src.Instructor.UserProfile != null ? $"{src.Instructor.UserProfile.FirstName} {src.Instructor.UserProfile.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.LessonsCount, opt => opt.MapFrom(src => src.Sections != null ? src.Sections.SelectMany(s => s.Lessons).Count() : 0))
                .ForMember(dest => dest.HasCertificate, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Sections, opt => opt.MapFrom(src => src.Sections))
                .ForMember(dest => dest.AvailableBatches, opt => opt.MapFrom(src => src.Batches));

            CreateMap<CreateCourseRequest, Courses>()
                .ForMember(dest => dest.slug, opt => opt.Ignore())   // set in service
                .ForMember(dest => dest.Status, opt => opt.Ignore()); // set in service
                // CourseAccessType and DefaultAssignmentDeadlineDays map by convention

            // ─── Section ─────────────────────────────────────────────────────
            CreateMap<CourseSection, SectionResponse>();
            CreateMap<CreateSectionRequest, CourseSection>();
            CreateMap<UpdateSectionRequest, CourseSection>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ─── Lesson ──────────────────────────────────────────────────────
            CreateMap<Lessons, LessonResponse>();
            CreateMap<Lessons, LessonDetailResponse>()
                .ForMember(dest => dest.Resources, opt => opt.MapFrom(src => src.Resources));
            CreateMap<CreateLessonRequest, Lessons>();
            CreateMap<UpdateLessonRequest, Lessons>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ─── Progress ────────────────────────────────────────────────────
            CreateMap<StudentProgress, LessonProgressResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.StudentId))
                .ForMember(dest => dest.LastViewedAt, opt => opt.MapFrom(src => src.LastAccessed))
                .ForMember(dest => dest.WatchPercentage, opt => opt.MapFrom(src => src.VideoWatchedPercentage))
                .ForMember(dest => dest.LastWatchedSecond, opt => opt.MapFrom(src => src.LastWatchedSecond));

            // ─── Resource ────────────────────────────────────────────────────
            CreateMap<LessonResources, ResourceResponse>();
            CreateMap<CreateResourceRequest, LessonResources>()
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore()); // set in service
            CreateMap<UpdateResourceRequest, LessonResources>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ─── Batch ─────────────────────────────────────────────────────
            // AvailableSeats is [NotMapped] and populated by the PostgreSQL function before mapping.
            CreateMap<CourseBatch, BatchResponse>();
            CreateMap<CourseBatch, BatchSummaryResponse>();
            CreateMap<CreateBatchRequest, CourseBatch>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())   // set in service
                .ForMember(dest => dest.CourseId, opt => opt.Ignore()); // set in service

            // ─── Enrollment ──────────────────────────────────────────────────
            CreateMap<Enrollments, EnrollmentResponse>()
                .ForMember(dest => dest.CourseTitle,
                    opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : string.Empty))
                .ForMember(dest => dest.BatchName,
                    opt => opt.MapFrom(src => src.Batch != null ? src.Batch.Name : null))
                .ForMember(dest => dest.ThumbnailUrl,
                    opt => opt.MapFrom(src => src.Course != null ? src.Course.ThumbnailUrl : null))
                .ForMember(dest => dest.InstructorName,
                    opt => opt.MapFrom(src => src.Course != null && src.Course.Instructor != null && src.Course.Instructor.UserProfile != null ? $"{src.Course.Instructor.UserProfile.FirstName} {src.Course.Instructor.UserProfile.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Course != null && src.Course.Category != null ? src.Course.Category.Name : string.Empty))
                .ForMember(dest => dest.LanguageName,
                    opt => opt.MapFrom(src => src.Course != null && src.Course.Language != null ? src.Course.Language.Name : string.Empty))
                .ForMember(dest => dest.Level,
                    opt => opt.MapFrom(src => src.Course != null ? src.Course.Level : LMSApi.ModelLibrary.Enums.CourseLevel.Beginner))
                .ForMember(dest => dest.LessonsCount,
                    opt => opt.MapFrom(src => src.Course != null && src.Course.Sections != null ? src.Course.Sections.SelectMany(s => s.Lessons).Count() : 0))
                .ForMember(dest => dest.EstimatedDuration,
                    opt => opt.MapFrom(src => src.Course != null ? src.Course.EstimatedDuration : TimeSpan.Zero))
                .ForMember(dest => dest.HasCertificate,
                    opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CourseAccessType,
                    opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseAccessType : LMSApi.ModelLibrary.Enums.CourseAccessType.SelfPaced));

            CreateMap<Certificates, CertificateResponse>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : string.Empty))
                .ForMember(dest => dest.LearnerName, opt => opt.MapFrom(src => src.User != null && src.User.UserProfile != null ? $"{src.User.UserProfile.FirstName} {src.User.UserProfile.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Course != null && src.Course.Instructor != null && src.Course.Instructor.UserProfile != null ? $"{src.Course.Instructor.UserProfile.FirstName} {src.Course.Instructor.UserProfile.LastName}".Trim() : string.Empty))
                .ForMember(dest => dest.CourseDescription, opt => opt.MapFrom(src => src.Course != null ? src.Course.Description : string.Empty))
                .ForMember(dest => dest.CourseThumbnailUrl, opt => opt.MapFrom(src => src.Course != null ? src.Course.ThumbnailUrl : string.Empty))
                .ForMember(dest => dest.CourseLevel, opt => opt.MapFrom(src => src.Course != null ? src.Course.Level.ToString() : string.Empty))
                .ForMember(dest => dest.CourseDurationHours, opt => opt.MapFrom(src => src.Course != null ? src.Course.EstimatedDuration.TotalHours : 0.0))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Course != null && src.Course.Category != null ? src.Course.Category.Name : string.Empty));
            CreateMap<CertificateTemplates,CertificateTemplateResponse>();
        }
    }
}
