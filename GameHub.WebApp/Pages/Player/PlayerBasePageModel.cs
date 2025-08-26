using GameHub.BLL.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace GameHub.WebApp.Pages.Player
{
	public abstract class PlayerBasePageModel : PageModel
	{
		protected readonly CurrentUserHelper _currentUserHelper;

		protected PlayerBasePageModel(CurrentUserHelper currentUserHelper)
		{
			_currentUserHelper = currentUserHelper;
		}

		/// <summary>
		/// Validates player access. Call this method at the beginning of OnGet/OnPost methods.
		/// </summary>
		/// <returns>IActionResult if redirect is needed, null if validation passes</returns>
		protected async Task<IActionResult?> ValidatePlayerAccessAsync()
		{
			// Check if user is logged in
			if (!_currentUserHelper.IsLoggedIn())
			{
				return RedirectToPage("/Login");
			}

			try
			{
				// Check if user is player (non-admin)
				if (!await _currentUserHelper.IsCurrentUserPlayerAsync())
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

		protected string BuildImageUrl(string? relativeOrAbsolutePath, IConfiguration configuration)
		{
			if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath)) return string.Empty;
			if (relativeOrAbsolutePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return relativeOrAbsolutePath;
			var baseUrl = configuration["BaseUrl"] ?? string.Empty;
			return $"{(string.IsNullOrEmpty(baseUrl) ? string.Empty : baseUrl.TrimEnd('/'))}/{relativeOrAbsolutePath.TrimStart('/')}";
		}
	}
}


