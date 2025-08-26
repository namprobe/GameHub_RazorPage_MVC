using GameHub.BLL.DTOs.Auth;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.WebApp.Helpers;
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
                this.SetValidationErrors(ModelState);
                return Page();
            }

            var result = await _authService.LoginAsync(LoginRequest);

            if (result.IsSuccess)
            {
                this.SetSuccessMessage(result.Message ?? "Login successful!");

                // Store avatarPath for navbar avatar
                if (result.Data != null && !string.IsNullOrEmpty(result.Data.AvatarPath))
                {
                    var options = new CookieOptions
                    {
                        IsEssential = true,
                        HttpOnly = false,
                        Secure = false,
                        Expires = result.Data.ExpiresAt
                    };
                    Response.Cookies.Append("avatarPath", result.Data.AvatarPath, options);
                }

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
                    this.SetErrorMessage("Authentication error occurred");
                    return Page();
                }
            }
            else
            {
                this.SetErrorMessage(result.Message ?? "Login failed. Please check your credentials.");
                return Page();
            }
        }
    }
}
