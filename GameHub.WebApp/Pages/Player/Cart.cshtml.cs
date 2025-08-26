using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.WebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Player
{
    public class CartModel : PlayerBasePageModel
    {
        private readonly ICartService _cartService;
        private readonly IGameService _gameService;
        public readonly IConfiguration Config;

        public CartModel(CurrentUserHelper currentUserHelper, ICartService cartService, IGameService gameService, IConfiguration config) : base(currentUserHelper)
        {
            _cartService = cartService;
            _gameService = gameService;
            Config = config;
        }

        [BindProperty(SupportsGet = true)]
        public BasePaginationFilter Filter { get; set; } = new BasePaginationFilter { Page = 1, PageSize = 6 };

        public BLL.DTOs.Cart.CartResponse? Cart { get; set; }
        public PaginationResult<GameHub.BLL.DTOs.Game.GameResponse>? RelatedGames { get; set; }
        [BindProperty(SupportsGet = true)]
        public int RelatedPage { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = await ValidatePlayerAccessAsync();
            if (redirect != null) return redirect;

            // Always fix cart items per page to 4
            Filter.PageSize = 4;
            Filter.Page = Math.Max(1, Filter.Page);

            var result = await _cartService.GetCurrentCartAsync(Filter);
            if (!result.IsSuccess)
            {
                this.SetErrorMessage(result.Message ?? "Failed to load cart");
                return Page();
            }

            Cart = result.Data;

            // Build related games based on distinct categories/developers in cart
            if (Cart != null && Cart.TotalItems > 0)
            {
                var categoryIds = Cart.CartItems
                    .Select(ci => ci.GameImagePath) // placeholder to avoid null ref; we'll fetch games by ids if needed
                    .ToList();
                // We do not have category/developer ids in CartItemResponse; fetch via a small page of games by ids
                var gameIds = Cart.CartItems.Select(ci => ci.GameId).Distinct().ToList();
                var filter = new GameHub.BLL.DTOs.Game.GameFilter
                {
                    Page = RelatedPage <= 0 ? 1 : RelatedPage,
                    PageSize = 4,
                    GameIds = gameIds // exclude these ids from results
                };

                // To derive CategoryIds/DeveloperIds we need a mini fetch for these game ids
                var currentGames = await _gameService.GetPagedAsync(new GameHub.BLL.DTOs.Game.GameFilter
                {
                    Page = 1,
                    PageSize = gameIds.Count == 0 ? 1 : gameIds.Count,
                    Title = null,
                });
                if (currentGames.IsSuccess)
                {
                    var cats = currentGames.Items.Where(g => g.Category?.Id != null).Select(g => g.Category!.Id).Distinct().ToList();
                    var devs = currentGames.Items.Where(g => g.Developer?.Id != null).Select(g => g.Developer!.Id).Distinct().ToList();
                    filter.CategoryIds = cats;
                    filter.DeveloperIds = devs;
                }

                RelatedGames = await _gameService.GetPagedAsync(filter);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int gameId)
        {
            var redirect = await ValidatePlayerAccessAsync();
            if (redirect != null) return redirect;

            var result = await _cartService.RemoveFromCartAsync(gameId);
            if (result.IsSuccess) this.SetSuccessMessage(result.Message ?? "Removed from cart");
            else this.SetErrorMessage(result.Message ?? "Failed to remove item");

            return RedirectToPage(new { Filter_Page = Filter.Page, Filter_PageSize = Filter.PageSize });
        }

        public async Task<IActionResult> OnPostClearAsync()
        {
            var redirect = await ValidatePlayerAccessAsync();
            if (redirect != null) return redirect;

            var result = await _cartService.ClearCartAsync();
            if (result.IsSuccess) this.SetSuccessMessage(result.Message ?? "Cart cleared");
            else this.SetErrorMessage(result.Message ?? "Failed to clear cart");

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int gameId)
        {
            var redirect = await ValidatePlayerAccessAsync();
            if (redirect != null) return redirect;

            var result = await _cartService.AddToCartAsync(gameId);
            if (result.IsSuccess) this.SetSuccessMessage(result.Message ?? "Added to cart");
            else this.SetErrorMessage(result.Message ?? "Failed to add to cart");

            return RedirectToPage(new { RelatedPage });
        }
    }
}


