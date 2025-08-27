using AutoMapper;
using GameHub.BLL.DTOs.GameRegistration;
using GameHub.BLL.DTOs.Payment;
using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.DTOs.Developer;
using GameHub.DAL.Entities;

namespace GameHub.BLL.Mapping
{
    public class GameRegistrationMappingProfile : Profile
    {
        public GameRegistrationMappingProfile()
        {
            CreateMap<GameRegistration, GameRegistrationResponse>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.PurchasePrice))
                .ForMember(dest => dest.GameRegistrationItems, opt => opt.MapFrom(src => src.GameRegistrationDetails))
                .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment));

            CreateMap<GameRegistrationDetail, GameRegistrationItem>()
                .ForMember(dest => dest.GameTitle, opt => opt.MapFrom(src => src.Game.Title))
                .ForMember(dest => dest.GameImagePath, opt => opt.MapFrom(src => src.Game.ImagePath ?? string.Empty))
                .ForMember(dest => dest.GameCategory, opt => opt.MapFrom(src => src.Game.Category))
                .ForMember(dest => dest.Developer, opt => opt.MapFrom(src => src.Game.Developer));

            CreateMap<GameCategory, GameCategoryItem>();
            
            CreateMap<Developer, DeveloperItem>();

            CreateMap<Payment, PaymentResponse>()
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()));
        }
    }
}