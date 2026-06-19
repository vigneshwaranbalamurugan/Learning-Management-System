using AutoMapper;
using LMSApi.ModelLibrary.DTOs.Notifications;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Mappers
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<Notifications, NotificationResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}
