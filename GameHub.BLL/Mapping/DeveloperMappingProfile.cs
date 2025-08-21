using AutoMapper;
using GameHub.BLL.DTOs.Developer;
using GameHub.DAL.Entities;

namespace GameHub.BLL.Mapping;

public class DeveloperMappingProfile : Profile
{
    public DeveloperMappingProfile()
    {
        // Entity to DTOs
        CreateMap<Developer, DeveloperItem>();
        CreateMap<Developer, DeveloperResponse>()
            .IncludeBase<Developer, DeveloperItem>();
        
        // Request to Entity
        CreateMap<DeveloperRequest, Developer>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Games, opt => opt.Ignore());
    }
}
