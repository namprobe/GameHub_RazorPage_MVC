using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.Developer
{
    public class CreateModel : AdminBasePageModel
    {
        private readonly IDeveloperService _developerService;

        public CreateModel(IDeveloperService developerService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _developerService = developerService;
        }

        [BindProperty]
        public DeveloperRequest DeveloperRequest { get; set; } = new();

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

            var result = await _developerService.CreateAsync(DeveloperRequest);
            
            if (result.IsSuccess)
            {
                SetSuccessMessage("Developer created successfully!");
                return RedirectToPage("./Index");
            }
            
            SetErrorMessage(result.Message ?? "Failed to create developer");
            return Page();
        }
    }
}
