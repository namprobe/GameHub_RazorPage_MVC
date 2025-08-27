using GameHub.BLL.DTOs.GameRegistration;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Player
{
    public class RegistrationsModel : PlayerBasePageModel
    {
        private readonly IGameRegistrationService _registrationService;
        public readonly IConfiguration Config;

        public RegistrationsModel(CurrentUserHelper currentUserHelper, IGameRegistrationService registrationService, IConfiguration config) : base(currentUserHelper)
        {
            _registrationService = registrationService;
            Config = config;
        }

        [BindProperty(SupportsGet = true)]
        public GameRegistrationFilter Filter { get; set; } = new GameRegistrationFilter
        {
            Page = 1,
            PageSize = 6,
            SortBy = nameof(GameHub.DAL.Entities.GameRegistration.RegistrationDate),
            IsAscending = false
        };

        public PaginationResult<GameRegistrationResponse> PageData { get; set; } = new PaginationResult<GameRegistrationResponse>();

        public async Task<IActionResult> OnGetAsync()
        {
            var access = await ValidatePlayerAccessAsync();
            if (access != null) return access;

            if (Filter.Page <= 0) Filter.Page = 1;
            if (Filter.PageSize <= 0) Filter.PageSize = 6;

            PageData = await _registrationService.GetPagedAsync(Filter);
            return Page();
        }
    }
}


