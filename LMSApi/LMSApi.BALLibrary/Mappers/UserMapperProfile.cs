using AutoMapper;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Mappers
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserProfiles, ProfileResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<Users, LMSApi.ModelLibrary.DTOs.UserManagement.AdminUserResponse>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.FirstName : string.Empty))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.LastName : string.Empty))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty));
        }
    }
}