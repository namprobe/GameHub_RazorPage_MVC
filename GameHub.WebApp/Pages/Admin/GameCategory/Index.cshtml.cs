using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.GameCategory
{
    public class IndexModel : AdminBasePageModel
    {
        private readonly IGameCategoryService _gameCategoryService;

        public IndexModel(IGameCategoryService gameCategoryService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _gameCategoryService = gameCategoryService;
        }

        public PaginationResult<GameCategoryResponse> GameCategories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public GameCategoryFilter Filter { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Validate admin access
            var authResult = await ValidateAdminAccessAsync();
            if (authResult != null) return authResult;

            // Set default pagination if not specified
            if (Filter.Page <= 0) Filter.Page = 1;
            if (Filter.PageSize <= 0) Filter.PageSize = 10;

            GameCategories = await _gameCategoryService.GetPagedAsync(Filter);

            return Page();
        }
    }
}
