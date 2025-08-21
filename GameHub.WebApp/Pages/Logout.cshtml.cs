using GameHub.BLL.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameHub.WebApp.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly CurrentUserHelper _currentUserHelper;

        public LogoutModel(CurrentUserHelper currentUserHelper)
        {
            _currentUserHelper = currentUserHelper;
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Login");
        }

        public IActionResult OnPost()
        {
            _currentUserHelper.Logout();
            return RedirectToPage("/Login");
        }
    }
}
