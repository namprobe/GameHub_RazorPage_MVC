using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.Developer
{
    public class EditModel : AdminBasePageModel
    {
        private readonly IDeveloperService _developerService;

        public EditModel(IDeveloperService developerService, CurrentUserHelper currentUserHelper) 
            : base(currentUserHelper)
        {
            _developerService = developerService;
        }

        [BindProperty]
        public DeveloperRequest DeveloperRequest { get; set; } = new();

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

            var result = await _developerService.GetByIdAsync(id.Value);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var developer = result.Data;
            DeveloperRequest = new DeveloperRequest
            {
                DeveloperName = developer.DeveloperName,
                Website = developer.Website
            };
            Id = developer.Id;

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

            var result = await _developerService.UpdateAsync(Id, DeveloperRequest);
            
            if (result.IsSuccess)
            {
                SetSuccessMessage("Developer updated successfully!");
                return RedirectToPage("./Index");
            }
            
            SetErrorMessage(result.Message ?? "Failed to update developer");
            return Page();
        }
    }
}
