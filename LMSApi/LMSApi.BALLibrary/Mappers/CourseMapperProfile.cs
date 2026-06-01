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

            // ─── Course ──────────────────────────────────────────────────────
            CreateMap<Courses, CourseResponse>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.slug))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));

            CreateMap<Courses, CourseDetailsResponse>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.slug))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.Sections, opt => opt.MapFrom(src => src.Sections));

            CreateMap<CreateCourseRequest, Courses>()
                .ForMember(dest => dest.slug, opt => opt.Ignore())   // set in service
                .ForMember(dest => dest.Status, opt => opt.Ignore()); // set in service

            // ─── Section ─────────────────────────────────────────────────────
            CreateMap<CourseSection, SectionResponse>();
            CreateMap<CreateSectionRequest, CourseSection>();
            CreateMap<UpdateSectionRequest, CourseSection>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ─── Lesson ──────────────────────────────────────────────────────
            CreateMap<Lessons, LessonResponse>();
            CreateMap<CreateLessonRequest, Lessons>();
            CreateMap<UpdateLessonRequest, Lessons>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ─── Resource ────────────────────────────────────────────────────
            CreateMap<LessonResources, ResourceResponse>();
            CreateMap<CreateResourceRequest, LessonResources>()
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore()); // set in service
            CreateMap<UpdateResourceRequest, LessonResources>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
