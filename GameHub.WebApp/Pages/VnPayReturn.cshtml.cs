using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.WebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages
{
    public class VnPayReturnModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        private readonly IGameRegistrationService _registrationService;
        private readonly CurrentUserHelper _currentUserHelper;

        public VnPayReturnModel(IGameRegistrationService registrationService, CurrentUserHelper currentUserHelper)
        {
            _registrationService = registrationService;
            _currentUserHelper = currentUserHelper;
        }

        public async Task<IActionResult> OnGet()
        {
            // Handle VNPay return regardless of user session state
            var result = await _registrationService.HandleVnPayReturnAsync(Request.Query);
            
            // Set the message
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message ?? (result.IsSuccess ? "Payment success" : "Payment failed");
            
            // Check if user is still logged in
            if (_currentUserHelper.IsLoggedIn())
            {
                try
                {
                    // If user is logged in and is a player, redirect to registrations
                    if (await _currentUserHelper.IsCurrentUserPlayerAsync())
                    {
                        return RedirectToPage("/Player/Registrations");
                    }
                }
                catch
                {
                    // If there's an error checking user status, logout and redirect to login
                    _currentUserHelper.Logout();
                }
            }
            
            // If user is not logged in or not a player, redirect to login page
            return RedirectToPage("/Login");
        }
    }
}


