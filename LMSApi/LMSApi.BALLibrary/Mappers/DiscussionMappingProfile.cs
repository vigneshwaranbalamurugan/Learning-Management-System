using AutoMapper;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
namespace LMSApi.BALLibrary.Mappers
{
    public class DiscussionMappingProfile : Profile
    {
        public DiscussionMappingProfile()
        {
            CreateMap<Discussions, DiscussionResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null && src.User.UserProfile != null ? $"{src.User.UserProfile.FirstName} {src.User.UserProfile.LastName}".Trim() : (src.User != null ? src.User.Email : string.Empty)))
                .ForMember(dest => dest.ReplyCount, opt => opt.Ignore())
                .ForMember(dest => dest.LikeCount, opt => opt.Ignore());

            CreateMap<Discussions, DiscussionDetailResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null && src.User.UserProfile != null ? $"{src.User.UserProfile.FirstName} {src.User.UserProfile.LastName}".Trim() : (src.User != null ? src.User.Email : string.Empty)))
                .ForMember(dest => dest.ReplyCount, opt => opt.Ignore())
                .ForMember(dest => dest.LikeCount, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore());

            CreateMap<DiscussionReplies, ReplyResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null && src.User.UserProfile != null ? $"{src.User.UserProfile.FirstName} {src.User.UserProfile.LastName}".Trim() : (src.User != null ? src.User.Email : string.Empty)));
        }
    }
}
