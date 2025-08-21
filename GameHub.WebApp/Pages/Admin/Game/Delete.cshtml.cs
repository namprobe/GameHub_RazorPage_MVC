using GameHub.BLL.DTOs.Game;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.Game
{
    public class DeleteModel : AdminBasePageModel
    {
        private readonly IGameService _gameService;

        public DeleteModel(IGameService gameService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _gameService = gameService;
        }

        public GameResponse Game { get; set; } = new();

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

            Game = result.Data;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            var authResult = await ValidateAdminAccessAsync();
            if (authResult != null) return authResult;

            if (id == null)
            {
                return NotFound();
            }

            var result = await _gameService.DeleteAsync(id.Value);
            
            if (result.IsSuccess)
            {
                SetSuccessMessage("Game deleted successfully!");
                return RedirectToPage("./Index");
            }
            
            SetErrorMessage(result.Message ?? "Failed to delete game");
            return RedirectToPage("./Index");
        }
    }
}
