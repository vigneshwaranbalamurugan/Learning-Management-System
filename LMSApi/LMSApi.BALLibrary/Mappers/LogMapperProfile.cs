using AutoMapper;
using LMSApi.ModelLibrary.DTOs.Logs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Mappers
{
    public class LogMappingProfile : Profile
    {
        public LogMappingProfile()
        {
            CreateMap<ActivityLogs, ActivityLogResponse>()
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => src.ActivityType.ToString()));

            CreateMap<AuditLogs, AuditLogResponse>()
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action.ToString()));
        }
    }
}
