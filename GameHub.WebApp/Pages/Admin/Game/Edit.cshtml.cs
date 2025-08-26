using GameHub.BLL.DTOs.Game;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GameHub.WebApp.Pages.Admin.Game
{
    public class EditModel : AdminBasePageModel
    {
        private readonly IGameService _gameService;
        private readonly IGameCategoryService _gameCategoryService;
        private readonly IDeveloperService _developerService;

        public EditModel(IGameService gameService, IGameCategoryService gameCategoryService, 
                        IDeveloperService developerService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _gameService = gameService;
            _gameCategoryService = gameCategoryService;
            _developerService = developerService;
        }

        [BindProperty]
        public GameRequest GameRequest { get; set; } = new();

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

            var result = await _gameService.GetByIdAsync(id.Value);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            // Map from GameResponse to GameRequest
            var game = result.Data;
            GameRequest = new GameRequest
            {
                Title = game.Title,
                Price = game.Price,
                ReleaseDate = game.ReleaseDate,
                CategoryId = game.Category?.Id ?? 0,
                DeveloperId = game.Developer?.Id ?? 0,
                Description = game.Description
            };
            Id = game.Id;

            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var authResult = await ValidateAdminAccessAsync();
            if (authResult != null) return authResult;

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            var result = await _gameService.UpdateAsync(Id, GameRequest);
            
            if (result.IsSuccess)
            {
                SetSuccessMessage("Game updated successfully!");
                return RedirectToPage("./Index");
            }
            
            SetErrorMessage(result.Message ?? "Failed to update game");
            await LoadSelectListsAsync();
            return Page();
        }

        private async Task LoadSelectListsAsync()
        {
            var categoriesResult = await _gameCategoryService.GetAllAsync();
            var developersResult = await _developerService.GetAllAsync();

            ViewData["CategoryId"] = categoriesResult.IsSuccess 
                ? new SelectList(categoriesResult.Data, "Id", "CategoryName", GameRequest.CategoryId)
                : new SelectList(new List<object>());

            ViewData["DeveloperId"] = developersResult.IsSuccess 
                ? new SelectList(developersResult.Data, "Id", "DeveloperName", GameRequest.DeveloperId)
                : new SelectList(new List<object>());
        }
    }
}
