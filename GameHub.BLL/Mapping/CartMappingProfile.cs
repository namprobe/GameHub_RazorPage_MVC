using AutoMapper;
using GameHub.BLL.DTOs.Cart;
using GameHub.DAL.Entities;

namespace GameHub.BLL.Mapping;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        CreateMap<Cart, CartResponse>();
        CreateMap<CartItem, CartItemResponse>();
    }
}