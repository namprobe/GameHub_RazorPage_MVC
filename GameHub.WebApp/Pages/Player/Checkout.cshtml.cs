using GameHub.BLL.DTOs.GameRegistration;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.WebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Player
{
    public class CheckoutModel : PlayerBasePageModel
    {
        private readonly ICartService _cartService;
        private readonly IGameRegistrationService _registrationService;
        private readonly IGameService _gameService;
        public readonly IConfiguration Config;

        public CheckoutModel(CurrentUserHelper currentUserHelper,
            ICartService cartService,
            IGameRegistrationService registrationService,
            IGameService gameService,
            IConfiguration config) : base(currentUserHelper)
        {
            _cartService = cartService;
            _registrationService = registrationService;
            _gameService = gameService;
            Config = config;
        }

        [BindProperty(SupportsGet = true)]
        public int? GameId { get; set; }

        [BindProperty]
        public string? PaymentMethod { get; set; } = "VNPay";

        public CartSummary? Summary { get; set; }
        public List<CheckoutItem> Items { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public BasePaginationFilter Filter { get; set; } = new BasePaginationFilter { Page = 1, PageSize = 4 };

        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }

        public class CheckoutItem
        {
            public int GameId { get; set; }
            public string Title { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string? ImagePath { get; set; }
        }

        public class CartSummary
        {
            public int TotalItems { get; set; }
            public decimal TotalPrice { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var access = await ValidatePlayerAccessAsync();
            if (access != null) return access;

            // If GameId is null -> checkout from cart; show items and summary
            if (!GameId.HasValue)
            {
                Filter.Page = Math.Max(1, Filter.Page);
                
                var res = await _cartService.GetCurrentCartAsync(Filter);
                if (res.IsSuccess && res.Data != null)
                {
                    Summary = new CartSummary
                    {
                        TotalItems = res.Data.TotalItems,
                        TotalPrice = res.Data.TotalPrice
                    };
                    if (res.Data.CartItems != null)
                    {
                        Items = res.Data.CartItems.Select(ci => new CheckoutItem
                        {
                            GameId = ci.GameId,
                            Title = ci.GameTitle,
                            Price = ci.Price,
                            ImagePath = ci.GameImagePath
                        }).ToList();
                        HasPrevious = Filter.Page > 1;
                        HasNext = (Filter.Page * Filter.PageSize) < res.Data.TotalItems;
                    }
                }
            }
            else
            {
                var gameRes = await _gameService.GetByIdAsync(GameId.Value);
                if (gameRes.IsSuccess && gameRes.Data != null)
                {
                    var g = gameRes.Data;
                    Items = new List<CheckoutItem> { new CheckoutItem { GameId = g.Id, Title = g.Title, Price = g.Price, ImagePath = g.ImagePath } };
                    Summary = new CartSummary { TotalItems = 1, TotalPrice = g.Price };
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var access = await ValidatePlayerAccessAsync();
            if (access != null) return access;

            var request = new GameRegistrationRequest
            {
                GameId = GameId,
                PaymentMethod = PaymentMethod
            };

            var result = await _registrationService.RegisterGameAsync(request);
            if (!result.IsSuccess || string.IsNullOrEmpty(result.Data))
            {
                this.SetErrorMessage(result.Message ?? "Failed to create payment");
                return RedirectToPage();
            }

            // Redirect to VNPay URL
            return Redirect(result.Data);
        }
    }
}


