using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameHub.WebApp.Helpers;

public static class ToastHelper
{
    public static void SetSuccessMessage(this PageModel pageModel, string message)
    {
        pageModel.TempData["SuccessMessage"] = message;
    }

    public static void SetErrorMessage(this PageModel pageModel, string message)
    {
        pageModel.TempData["ErrorMessage"] = message;
    }

    public static void SetWarningMessage(this PageModel pageModel, string message)
    {
        pageModel.TempData["WarningMessage"] = message;
    }

    public static void SetInfoMessage(this PageModel pageModel, string message)
    {
        pageModel.TempData["InfoMessage"] = message;
    }

    public static void SetValidationErrors(this PageModel pageModel, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        var errors = modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        if (errors.Any())
        {
            pageModel.SetErrorMessage(string.Join(", ", errors));
        }
    }
}
