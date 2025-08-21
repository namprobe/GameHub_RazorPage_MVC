using GameHub.BLL.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameHub.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly CurrentUserHelper _currentUserHelper;

        public IndexModel(ILogger<IndexModel> logger, CurrentUserHelper currentUserHelper)
        {
            _logger = logger;
            _currentUserHelper = currentUserHelper;
        }

        public async Task<IActionResult> OnGet()
        {
            // Check if user is logged in
            if (_currentUserHelper.IsLoggedIn())
            {
                try
                {
                    // Check if user is admin
                    if (await _currentUserHelper.IsCurrentUserAdminAsync())
                    {
                        return RedirectToPage("/Admin/Game/Index");
                    }
                    else
                    {
                        // Not admin, logout and redirect to login
                        _currentUserHelper.Logout();
                        return RedirectToPage("/Login");
                    }
                }
                catch
                {
                    // Error checking admin status, logout and redirect to login
                    _currentUserHelper.Logout();
                    return RedirectToPage("/Login");
                }
            }

            // Not logged in, redirect to login
            return RedirectToPage("/Login");
        }
    }
}
