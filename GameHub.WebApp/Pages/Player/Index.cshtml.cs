using GameHub.BLL.DTOs.Game;
using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using Microsoft.AspNetCore.Mvc;
using GameHub.BLL.Helpers;
using GameHub.WebApp.Helpers;

namespace GameHub.WebApp.Pages.Player
{
	public class IndexModel : PlayerBasePageModel
	{
		private readonly IGameService _gameService;
		private readonly IGameCategoryService _gameCategoryService;
		private readonly IDeveloperService _developerService;
        private readonly ICartService _cartService;

		public IndexModel(
			IGameService gameService,
			IGameCategoryService gameCategoryService,
			IDeveloperService developerService,
			CurrentUserHelper currentUserHelper,
			ICartService cartService) : base(currentUserHelper)
		{
			_gameService = gameService;
			_gameCategoryService = gameCategoryService;
			_developerService = developerService;
            _cartService = cartService;
		}

		// Top registered games
		public PaginationResult<GameResponse> TopRegisteredGames { get; set; } = new();
		// Featured Games - Latest Games
		public PaginationResult<GameResponse> LatestGames { get; set; } = new();
		
		// Best Price Games
		public PaginationResult<GameResponse> BestPriceGames { get; set; } = new();
		
		// Popular Categories
		public IEnumerable<GameCategoryItem> Categories { get; set; } = new List<GameCategoryItem>();
		
		// Featured Developers
		public IEnumerable<DeveloperItem> Developers { get; set; } = new List<DeveloperItem>();

		public async Task<IActionResult> OnGetAsync()
		{
			var access = await ValidatePlayerAccessAsync();
			if (access != null) return access;

			// Top registered games (by RegistrationCount DESC, limit 6)
			var topGamesFilter = new GameFilter
			{
				Page = 1,
				PageSize = 3,
				SortBy = "RegistrationCount",
				IsAscending = false,
				IsActive = true
			};
			TopRegisteredGames = await _gameService.GetPagedAsync(topGamesFilter);

			// Load Latest Games (sorted by CreatedAt DESC, limit 6)
			var latestGamesFilter = new GameFilter
			{
				Page = 1,
				PageSize = 3,
				SortBy = "CreatedAt",
				IsAscending = false, // DESC để lấy mới nhất
				IsActive = true
			};
			LatestGames = await _gameService.GetPagedAsync(latestGamesFilter);

			// Load Best Price Games (sorted by Price ASC, limit 6)
			var bestPriceFilter = new GameFilter
			{
				Page = 1,
				PageSize = 3,
				SortBy = "Price",
				IsAscending = true, // ASC để lấy giá thấp nhất
				IsActive = true
			};
			BestPriceGames = await _gameService.GetPagedAsync(bestPriceFilter);

			// Load Categories
			var categoriesResult = await _gameCategoryService.GetAllAsync();
			if (categoriesResult.IsSuccess)
			{
				Categories = categoriesResult.Data ?? new List<GameCategoryItem>();
			}

			// Load Developers
			var developersResult = await _developerService.GetAllAsync();
			if (developersResult.IsSuccess)
			{
				Developers = developersResult.Data ?? new List<DeveloperItem>();
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

			return RedirectToPage();
		}
	}
}

