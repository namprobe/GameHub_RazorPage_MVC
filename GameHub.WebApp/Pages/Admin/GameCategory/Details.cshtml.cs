using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.GameCategory
{
    public class DetailsModel : AdminBasePageModel
    {
        private readonly IGameCategoryService _gameCategoryService;

        public DetailsModel(IGameCategoryService gameCategoryService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _gameCategoryService = gameCategoryService;
        }

        public GameCategoryResponse GameCategory { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var authResult = await ValidateAdminAccessAsync();
            if (authResult != null) return authResult;

            if (id == null)
            {
                return NotFound();
            }

            var result = await _gameCategoryService.GetByIdAsync(id.Value);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            GameCategory = result.Data;
            return Page();
        }
    }
}
