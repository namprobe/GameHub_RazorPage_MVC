using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.GameCategory
{
    public class CreateModel : AdminBasePageModel
    {
        private readonly IGameCategoryService _gameCategoryService;

        public CreateModel(IGameCategoryService gameCategoryService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _gameCategoryService = gameCategoryService;
        }

        [BindProperty]
        public GameCategoryRequest GameCategoryRequest { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var authResult = await ValidateAdminAccessAsync();
            if (authResult != null) return authResult;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var authResult = await ValidateAdminAccessAsync();
            if (authResult != null) return authResult;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _gameCategoryService.CreateAsync(GameCategoryRequest);
            
            if (result.IsSuccess)
            {
                SetSuccessMessage("Game Category created successfully!");
                return RedirectToPage("./Index");
            }
            
            SetErrorMessage(result.Message ?? "Failed to create category");
            return Page();
        }
    }
}
