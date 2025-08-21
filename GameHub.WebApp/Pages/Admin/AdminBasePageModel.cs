using GameHub.BLL.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameHub.WebApp.Pages.Admin
{
    public abstract class AdminBasePageModel : PageModel
    {
        protected readonly CurrentUserHelper _currentUserHelper;

        protected AdminBasePageModel(CurrentUserHelper currentUserHelper)
        {
            _currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// Validates admin access. Call this method at the beginning of OnGet/OnPost methods.
        /// </summary>
        /// <returns>IActionResult if redirect is needed, null if validation passes</returns>
        protected async Task<IActionResult?> ValidateAdminAccessAsync()
        {
            // Check if user is logged in
            if (!_currentUserHelper.IsLoggedIn())
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Check if user is admin
                if (!await _currentUserHelper.IsCurrentUserAdminAsync())
                {
                    _currentUserHelper.Logout();
                    return RedirectToPage("/Login");
                }
            }
            catch
            {
                _currentUserHelper.Logout();
                return RedirectToPage("/Login");
            }

            return null; // Validation passed
        }

        /// <summary>
        /// Sets error message in TempData
        /// </summary>
        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        /// <summary>
        /// Sets success message in TempData
        /// </summary>
        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }
    }
}
