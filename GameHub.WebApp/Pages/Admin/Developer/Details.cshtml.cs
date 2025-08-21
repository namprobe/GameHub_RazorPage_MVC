using GameHub.BLL.DTOs.Developer;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Admin.Developer
{
    public class DetailsModel : AdminBasePageModel
    {
        private readonly IDeveloperService _developerService;

        public DetailsModel(IDeveloperService developerService, CurrentUserHelper currentUserHelper) 
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
    }
}
