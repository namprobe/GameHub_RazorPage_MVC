using GameHub.BLL.DTOs.Game;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.Game
{
    public class DetailsModel : AdminBasePageModel
    {
        private readonly IGameService _gameService;

        public DetailsModel(IGameService gameService, CurrentUserHelper currentUserHelper) 
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

        // AJAX handler to get updated game details for real-time updates
        public async Task<IActionResult> OnGetGameDetailsAsync(int id)
        {
            var authResult = await ValidateAdminAccessAsync();
            if (authResult != null) return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            var result = await _gameService.GetByIdAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                return new JsonResult(new { error = "Game not found" }) { StatusCode = 404 };
            }

            Game = result.Data;
            
            // Return partial view as HTML string for DOM update
            var partialHtml = $@"
                <div class=""card-header"">
                    <h4 class=""mb-0"">{Game.Title}</h4>
                </div>
                <div class=""card-body"">
                    <div class=""row"">
                        <div class=""col-md-6"">
                            <dl class=""row"">
                                <dt class=""col-sm-4"">Title:</dt>
                                <dd class=""col-sm-8"">{Game.Title}</dd>
                                
                                <dt class=""col-sm-4"">Price:</dt>
                                <dd class=""col-sm-8"">${Game.Price:F2}</dd>
                                
                                <dt class=""col-sm-4"">Release Date:</dt>
                                <dd class=""col-sm-8"">{(Game.ReleaseDate?.ToString("yyyy-MM-dd") ?? "Not set")}</dd>
                            </dl>
                        </div>
                        <div class=""col-md-6"">
                            <dl class=""row"">
                                <dt class=""col-sm-4"">Category:</dt>
                                <dd class=""col-sm-8"">{Game.Category?.CategoryName ?? "No Category"}</dd>
                                
                                <dt class=""col-sm-4"">Developer:</dt>
                                <dd class=""col-sm-8"">{Game.Developer?.DeveloperName ?? "No Developer"}</dd>
                                
                                <dt class=""col-sm-4"">Status:</dt>
                                <dd class=""col-sm-8"">
                                    <span class=""badge {(Game.IsActive ? "bg-success" : "bg-secondary")}"">
                                        {(Game.IsActive ? "Active" : "Inactive")}
                                    </span>
                                </dd>
                                
                                <dt class=""col-sm-4"">Registered Players:</dt>
                                <dd class=""col-sm-8"">
                                    <span class=""badge bg-info fs-6"">
                                        <i class=""fas fa-users me-1""></i>
                                        {Game.PlayerCount} players
                                    </span>
                                </dd>
                                
                                <dt class=""col-sm-4"">Game ID:</dt>
                                <dd class=""col-sm-8"">#{Game.Id}</dd>
                            </dl>
                        </div>
                    </div>
                </div>
                <div class=""card-footer"">
                    <div class=""d-flex justify-content-between"">
                        <a href=""/Admin/Game/Delete/{Game.Id}"" class=""btn btn-danger"">Delete Game</a>
                        <div>
                            <a href=""/Admin/Game/Edit/{Game.Id}"" class=""btn btn-warning me-2"">Edit Game</a>
                            <a href=""/Admin/Game/Index"" class=""btn btn-secondary"">Back to List</a>
                        </div>
                    </div>
                </div>";

            return Content(partialHtml, "text/html");
        }
    }
}
