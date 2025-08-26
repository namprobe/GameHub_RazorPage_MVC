using GameHub.BLL.DTOs.Game;
using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using Microsoft.AspNetCore.Mvc;
using GameHub.WebApp.Helpers;

namespace GameHub.WebApp.Pages.Player
{
	public class GamesModel : PlayerBasePageModel
	{
		private readonly IGameService _gameService;
		private readonly IGameCategoryService _gameCategoryService;
		private readonly IDeveloperService _developerService;

        

		public PaginationResult<GameResponse> Games { get; set; } = new();

		[BindProperty(SupportsGet = true)]
		public GameFilter Filter { get; set; } = new();

		public IEnumerable<GameCategoryItem> Categories { get; set; } = new List<GameCategoryItem>();
		public IEnumerable<DeveloperItem> Developers { get; set; } = new List<DeveloperItem>();


		public async Task<IActionResult> OnGetAsync()
		{
			var access = await ValidatePlayerAccessAsync();
			if (access != null) return access;

			// Set defaults
			if (Filter.Page <= 0) Filter.Page = 1;
			if (Filter.PageSize <= 0) Filter.PageSize = 12;
			if (string.IsNullOrEmpty(Filter.SortBy)) Filter.SortBy = "CreatedAt";
			if (!Filter.IsAscending.HasValue) Filter.IsAscending = false; // Default DESC

			// Load dropdown data
			var categoriesResult = await _gameCategoryService.GetAllAsync();
			if (categoriesResult.IsSuccess)
			{
				Categories = categoriesResult.Data ?? new List<GameCategoryItem>();
			}

			var developersResult = await _developerService.GetAllAsync();
			if (developersResult.IsSuccess)
			{
				Developers = developersResult.Data ?? new List<DeveloperItem>();
			}

			// Load games with filter
			Games = await _gameService.GetPagedAsync(Filter);


			return Page();
		}

		private readonly ICartService _cartService;

		public GamesModel(
			IGameService gameService,
			IGameCategoryService gameCategoryService,
			IDeveloperService developerService,
			GameHub.BLL.Helpers.CurrentUserHelper currentUserHelper,
			ICartService cartService) : base(currentUserHelper)
		{
			_gameService = gameService;
			_gameCategoryService = gameCategoryService;
			_developerService = developerService;
			_cartService = cartService;
		}

		public async Task<IActionResult> OnPostAddToCartAsync(int gameId)
		{
			var access = await ValidatePlayerAccessAsync();
			if (access != null) return access;

			var result = await _cartService.AddToCartAsync(gameId);
			if (result.IsSuccess) this.SetSuccessMessage(result.Message ?? "Added to cart");
			else this.SetErrorMessage(result.Message ?? "Failed to add to cart");

			return RedirectToPage(new {
				Filter_Page = Filter.Page,
				Filter_PageSize = Filter.PageSize,
				Filter_Title = Filter.Title,
				Filter_CategoryId = Filter.CategoryId,
				Filter_DeveloperId = Filter.DeveloperId,
				Filter_SortBy = Filter.SortBy,
				Filter_IsAscending = Filter.IsAscending
			});
		}
	}
}
