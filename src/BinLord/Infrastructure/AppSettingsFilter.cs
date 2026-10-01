using BinLord.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BinLord.Infrastructure;

/// <summary>
/// Loads the configured app name into ViewBag before every action, so
/// layouts and views can show it without each controller fetching it.
/// </summary>
public class AppSettingsFilter : IAsyncActionFilter
{
    private readonly SettingsService _settingsService;

    public AppSettingsFilter(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is Controller controller)
        {
            var settings = await _settingsService.GetAsync();
            controller.ViewBag.AppName = settings.AppName;
        }

        await next();
    }
}
