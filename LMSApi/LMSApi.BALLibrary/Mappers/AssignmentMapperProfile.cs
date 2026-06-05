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

            // ─── Submission ───────────────────────────────────────────────
            CreateMap<AssignmentSubmissions, AssignmentSubmissionResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

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
