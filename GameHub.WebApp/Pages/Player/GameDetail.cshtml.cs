using GameHub.BLL.DTOs.Game;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using GameHub.WebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Player
{
    public class GameDetailModel : PlayerBasePageModel
    {
        private readonly IGameService _gameService;
        private readonly ICartService _cartService;

        public GameDetailModel(CurrentUserHelper currentUserHelper, IGameService gameService, ICartService cartService) : base(currentUserHelper)
        {
            _gameService = gameService;
            _cartService = cartService;
        }

        public GameResponse? Game { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public int RelatedPage { get; set; } = 1;

        public PaginationResult<GameResponse>? RelatedGames { get; set; }
        public bool IsCurrentGameInCart { get; set; }
        public HashSet<int> RelatedInCartGameIds { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var access = await ValidatePlayerAccessAsync();
            if (access != null) return access;

            var gameRes = await _gameService.GetByIdAsync(Id);
            if (!gameRes.IsSuccess || gameRes.Data == null)
            {
                this.SetErrorMessage(gameRes.Message ?? "Game not found");
                return RedirectToPage("/Player/Games");
            }

            Game = gameRes.Data;

            // Check if current game is already in cart
            IsCurrentGameInCart = await _cartService.IsInCurrentCartAsync(Id);

            var rf = new GameFilter
            {
                Page = RelatedPage <= 0 ? 1 : RelatedPage,
                PageSize = 4,
                CategoryId = Game.Category?.Id,
                DeveloperId = Game.Developer?.Id,
                IsActive = true,
                SortBy = "RegistrationCount",
                IsAscending = false,
                GameIds = new List<int> { Id } // exclude current game from related list
            };
            RelatedGames = await _gameService.GetPagedAsync(rf);

            // Populate in-cart flags for related items
            if (RelatedGames != null && RelatedGames.IsSuccess && RelatedGames.Items != null)
            {
                foreach (var g in RelatedGames.Items)
                {
                    if (await _cartService.IsInCurrentCartAsync(g.Id)) RelatedInCartGameIds.Add(g.Id);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int gameId)
        {
            var access = await ValidatePlayerAccessAsync();
            if (access != null) return access;

            var result = await _cartService.AddToCartAsync(gameId);
            if (result.IsSuccess) this.SetSuccessMessage(result.Message ?? "Added to cart");
            else this.SetErrorMessage(result.Message ?? "Failed to add to cart");

            return RedirectToPage(new { id = Id, RelatedPage });
        }

        public async Task<IActionResult> OnPostRegisterNowAsync()
        {
            var access = await ValidatePlayerAccessAsync();
            if (access != null) return access;
            this.SetInfoMessage("Register feature will be implemented soon.");
            return RedirectToPage(new { id = Id, RelatedPage });
        }
    }
}


