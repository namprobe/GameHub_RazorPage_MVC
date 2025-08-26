using AutoMapper;
using GameHub.BLL.DTOs.Cart;
using GameHub.DAL.Entities;

namespace GameHub.BLL.Mapping;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        // Only structural mapping here. Business-calculated fields (TotalItems, TotalPrice) are set in service.
        CreateMap<Cart, CartResponse>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.CartItems.Sum(x => x.Game.Price)));
        CreateMap<CartItem, CartItemResponse>()
            .ForMember(dest => dest.GameTitle, opt => opt.MapFrom(src => src.Game.Title))
            .ForMember(dest => dest.GameImagePath, opt => opt.MapFrom(src => src.Game.ImagePath))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Game.Price))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}