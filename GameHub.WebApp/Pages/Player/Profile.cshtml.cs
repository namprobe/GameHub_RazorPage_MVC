using GameHub.BLL.DTOs.Auth;
using GameHub.BLL.Helpers;
using GameHub.BLL.Interfaces;
using GameHub.WebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GameHub.WebApp.Pages.Player
{
	public class ProfileModel : PlayerBasePageModel
	{
		private readonly IAuthService _authService;

		public ProfileModel(IAuthService authService, CurrentUserHelper currentUserHelper) : base(currentUserHelper)
		{
			_authService = authService;
		}

		[BindProperty]
		public UpdateProfileRequest Form { get; set; } = new();

		public ProfileResponse? Profile { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			var access = await ValidatePlayerAccessAsync();
			if (access != null) return access!;

			var result = await _authService.GetProfileAsync();
			if (!result.IsSuccess)
			{
				ToastHelper.SetErrorMessage(this, result.Message ?? "Failed to load profile");
				return RedirectToPage("/Player/Index");
			}

			Profile = result.Data;
			Form.Username = Profile!.Username;
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			var access = await ValidatePlayerAccessAsync();
			if (access != null) return access!;

			if (!ModelState.IsValid)
			{
				ToastHelper.SetValidationErrors(this, ModelState);
				await LoadProfileAsync();
				return Page();
			}

			var update = await _authService.UpdateProfileAsync(Form);
			if (!update.IsSuccess)
			{
				ToastHelper.SetErrorMessage(this, update.Message ?? "Failed to update profile");
				await LoadProfileAsync();
				return Page();
			}

			ToastHelper.SetSuccessMessage(this, update.Message ?? "Profile updated successfully");
			// Refresh header by redirecting back to profile; _Layout will re-fetch profile for avatar/username
			return RedirectToPage("/Player/Profile");
		}

		private async Task LoadProfileAsync()
		{
			var result = await _authService.GetProfileAsync();
			if (result.IsSuccess)
			{
				Profile = result.Data;
			}
		}
	}
}
