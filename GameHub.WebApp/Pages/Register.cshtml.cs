using GameHub.BLL.DTOs.Auth;
using GameHub.BLL.Interfaces;
using GameHub.WebApp.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameHub.WebApp.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterRequest RegisterRequest { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // Clear any cached messages
            ErrorMessage = null;
            SuccessMessage = null;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                this.SetValidationErrors(ModelState);
                return Page();
            }

            var result = await _authService.RegisterAsync(RegisterRequest);

            if (result.IsSuccess)
            {
                this.SetSuccessMessage(result.Message ?? "Registration successful! Please login to continue.");
                return RedirectToPage("/Login");
            }
            else
            {
                this.SetErrorMessage(result.Message ?? "Registration failed. Please try again.");
                return Page();
            }
        }
    }
}
