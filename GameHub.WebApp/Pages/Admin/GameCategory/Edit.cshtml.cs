using GameHub.BLL.DTOs.GameCategory;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.GameCategory
{
    public class EditModel : AdminBasePageModel
    {
        private readonly IGameCategoryService _gameCategoryService;

        public EditModel(IGameCategoryService gameCategoryService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _gameCategoryService = gameCategoryService;
        }

        [BindProperty]
        public GameCategoryRequest GameCategoryRequest { get; set; } = new();

        [BindProperty]
        public int Id { get; set; }

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

            var category = result.Data;
            GameCategoryRequest = new GameCategoryRequest
            {
                CategoryName = category.CategoryName,
                Description = category.Description
            };
            Id = category.Id;

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

            var result = await _gameCategoryService.UpdateAsync(Id, GameCategoryRequest);
            
            if (result.IsSuccess)
            {
                SetSuccessMessage("Game Category updated successfully!");
                return RedirectToPage("./Index");
            }
            
            SetErrorMessage(result.Message ?? "Failed to update category");
            return Page();
        }
    }
}
