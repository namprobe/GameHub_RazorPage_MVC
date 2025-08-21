using GameHub.BLL.DTOs.Auth;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameHub.WebApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly CurrentUserHelper _currentUserHelper;

        public LoginModel(IAuthService authService, CurrentUserHelper currentUserHelper)
        {
            _authService = authService;
            _currentUserHelper = currentUserHelper;
        }

        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Clear any cached messages
            ErrorMessage = null;
            SuccessMessage = null;

            // If user is already logged in, redirect by role
            if (_currentUserHelper.IsLoggedIn())
            {
                bool isAdmin = false;
                try
                {
                    isAdmin = await _currentUserHelper.IsCurrentUserAdminAsync();
                }
                catch
                {
                    // fallback to login page if any error occurs
                    return Page();
                }

                if (isAdmin)
                {
                    return RedirectToPage("/Admin/Game/Index");
                }
                else
                {
                    return RedirectToPage("/Player/Index");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.LoginAsync(LoginRequest);

            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;

                // Redirect by role
                try
                {
                    if (await _currentUserHelper.IsCurrentUserAdminAsync())
                    {
                        return RedirectToPage("/Admin/Game/Index");
                    }
                    else
                    {
                        return RedirectToPage("/Player/Index");
                    }
                }
                catch
                {
                    ErrorMessage = "Authentication error occurred";
                    return Page();
                }
            }
            else
            {
                ErrorMessage = result.Message ?? "Login failed";
                return Page();
            }
        }
    }
}
