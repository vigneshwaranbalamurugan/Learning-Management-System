using AutoMapper;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.BALLibrary.Mappers
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<PlatformFeeConfig, PlatformFeeResponse>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.FeeCategory.ToString()))
                .ForMember(dest => dest.FeeType, opt => opt.MapFrom(src => src.FeeType.ToString()))
                .ForMember(dest => dest.CreatedByAdminEmail, opt => opt.MapFrom(src => src.CreatedByAdmin != null ? src.CreatedByAdmin.Email : string.Empty))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.FeeType == FeeType.Percentage 
                    ? $"{src.Value}% of course price" 
                    : $"₹{src.Value} flat fee"));

            CreateMap<InstructorPayout, InstructorPayoutResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RazorpayTransferId, opt => opt.MapFrom(src => src.RazorpayPayoutId))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Payment != null && src.Payment.Course != null ? src.Payment.Course.Title : string.Empty))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Payment != null && src.Payment.User != null && src.Payment.User.UserProfile != null 
                    ? $"{src.Payment.User.UserProfile.FirstName} {src.Payment.User.UserProfile.LastName}" 
                    : null));

            CreateMap<InstructorPayoutAccount, PayoutAccountResponse>()
                .ForMember(dest => dest.IsRouteReady, opt => opt.MapFrom(src => src.IsActive && !string.IsNullOrEmpty(src.RazorpayLinkedAccountId)));

            CreateMap<InstructorLinkedAccount, LinkedAccountResponse>()
                .ForMember(d => d.HasStakeholder, o => o.MapFrom(s => s.Stakeholder != null))
                .ForMember(d => d.HasProduct, o => o.MapFrom(s => s.PayoutProduct != null))
                .ForMember(d => d.IsBankConfigured, o => o.MapFrom(s => s.PayoutProduct != null && s.PayoutProduct.TncAccepted));

            CreateMap<InstructorStakeholder, StakeholderResponse>();

            CreateMap<InstructorPayoutProduct, PayoutProductResponse>()
                .ForMember(d => d.AccountNumber, o => o.MapFrom(s =>
                    s.AccountNumber.Length > 4
                        ? "****" + s.AccountNumber.Substring(s.AccountNumber.Length - 4)
                        : s.AccountNumber));
        }
    }
}
