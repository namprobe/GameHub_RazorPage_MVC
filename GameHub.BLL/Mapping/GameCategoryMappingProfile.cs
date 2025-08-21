using AutoMapper;
using GameHub.BLL.DTOs.GameCategory;
using GameHub.DAL.Entities;

namespace GameHub.BLL.Mapping;

public class GameCategoryMappingProfile : Profile
{
    public GameCategoryMappingProfile()
    {
        // Entity to DTOs - Handle inheritance properly
        CreateMap<GameCategory, GameCategoryItem>();
        CreateMap<GameCategory, GameCategoryResponse>()
            .IncludeBase<GameCategory, GameCategoryItem>();
        
        // Request to Entity
        CreateMap<GameCategoryRequest, GameCategory>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Games, opt => opt.Ignore());
    }
}