using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.Developer
{
    public class IndexModel : AdminBasePageModel
    {
        private readonly IDeveloperService _developerService;

        public IndexModel(IDeveloperService developerService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _developerService = developerService;
        }

        public PaginationResult<DeveloperResponse> Developers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DeveloperFilter Filter { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Validate admin access
            var authResult = await ValidateAdminAccessAsync();
            if (authResult != null) return authResult;

            // Set default pagination if not specified
            if (Filter.Page <= 0) Filter.Page = 1;
            if (Filter.PageSize <= 0) Filter.PageSize = 10;

            Developers = await _developerService.GetPagedAsync(Filter);

            return Page();
        }
    }
}
