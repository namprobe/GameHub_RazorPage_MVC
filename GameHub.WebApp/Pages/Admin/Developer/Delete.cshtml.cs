using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.Developer
{
    public class DeleteModel : AdminBasePageModel
    {
        private readonly IDeveloperService _developerService;

        public DeleteModel(IDeveloperService developerService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _developerService = developerService;
        }

        public DeveloperResponse Developer { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var authResult = await ValidateAdminAccessAsync();
            if (authResult != null) return authResult;

            if (id == null)
            {
                return NotFound();
            }

            var result = await _developerService.GetByIdAsync(id.Value);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            Developer = result.Data;
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

            var result = await _developerService.DeleteAsync(id.Value);
            
            if (result.IsSuccess)
            {
                SetSuccessMessage("Developer deleted successfully!");
                return RedirectToPage("./Index");
            }
            
            SetErrorMessage(result.Message ?? "Failed to delete developer");
            return RedirectToPage("./Index");
        }
    }
}
