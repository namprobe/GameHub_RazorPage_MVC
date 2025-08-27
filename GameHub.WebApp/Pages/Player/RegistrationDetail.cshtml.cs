using GameHub.BLL.DTOs.GameRegistration;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Player
{
    public class RegistrationDetailModel : PlayerBasePageModel
    {
        private readonly IGameRegistrationService _registrationService;
        public readonly IConfiguration Config;

        public RegistrationDetailModel(CurrentUserHelper currentUserHelper, IGameRegistrationService registrationService, IConfiguration config) : base(currentUserHelper)
        {
            _registrationService = registrationService;
            Config = config;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public GameRegistrationResponse? Data { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var access = await ValidatePlayerAccessAsync();
            if (access != null) return access;

            var res = await _registrationService.GetByIdAsync(Id);
            if (!res.IsSuccess)
                return RedirectToPage("/Player/Registrations");

            Data = res.Data;
            return Page();
        }
    }
}


