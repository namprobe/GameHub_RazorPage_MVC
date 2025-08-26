using AutoMapper;
using GameHub.BLL.DTOs.Game;
using GameHub.DAL.Entities;

namespace GameHub.BLL.Mapping;

public class GameMappingProfile : Profile
{
    public GameMappingProfile()
    {
        // Entity to DTOs
        CreateMap<Game, GameResponse>()
            .ForMember(dest => dest.Developer, opt => opt.MapFrom(src => src.Developer))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
        
        // Request to Entity
        CreateMap<GameRequest, Game>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ImagePath, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Developer, opt => opt.Ignore())
            .ForMember(dest => dest.GameRegistrationDetails, opt => opt.Ignore())
            .ForMember(dest => dest.CartItems, opt => opt.Ignore())
            .ForMember(dest => dest.RegistrationCount, opt => opt.Ignore());
    }
}
