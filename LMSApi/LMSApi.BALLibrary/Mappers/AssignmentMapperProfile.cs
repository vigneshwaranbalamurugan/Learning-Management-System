using AutoMapper;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Mappers
{
    public class AssignmentMapperProfile : Profile
    {
        public AssignmentMapperProfile()
        {
            // ─── Assignment ────────────────────────────────────────────────
            CreateMap<Assignments, AssignmentResponse>();

            CreateMap<CreateAssignmentRequest, Assignments>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())  // set by DbContext
                .ForMember(dest => dest.Submissions, opt => opt.Ignore());

            CreateMap<UpdateAssignmentRequest, Assignments>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AssignmentSubmissions, AssignmentSubmissionResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => 
                    src.Student != null 
                        ? (src.Student.UserProfile != null && (!string.IsNullOrEmpty(src.Student.UserProfile.FirstName) || !string.IsNullOrEmpty(src.Student.UserProfile.LastName))
                            ? (src.Student.UserProfile.FirstName + " " + src.Student.UserProfile.LastName).Trim() 
                            : LMSApi.BALLibrary.Utils.MaskingUtils.MaskEmail(src.Student.Email)) 
                        : null))
                .ForMember(dest => dest.StudentEmail, opt => opt.MapFrom(src => src.Student != null ? LMSApi.BALLibrary.Utils.MaskingUtils.MaskEmail(src.Student.Email) : null));

            CreateMap<AssignmentSubmissionRequest, AssignmentSubmissions>()
                .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())  // set in service
                .ForMember(dest => dest.Status, opt => opt.Ignore())        // set in service
                .ForMember(dest => dest.AttemptNumber, opt => opt.Ignore()) // set in service
                .ForMember(dest => dest.IsPassed, opt => opt.Ignore())      // set after grading
                .ForMember(dest => dest.MarksAwarded, opt => opt.Ignore())
                .ForMember(dest => dest.Feedback, opt => opt.Ignore())
                .ForMember(dest => dest.GradedAt, opt => opt.Ignore());
        }
    }
}
