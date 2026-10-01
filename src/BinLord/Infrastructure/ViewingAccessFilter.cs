using BinLord.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BinLord.Infrastructure;

/// <summary>
/// Gate for read-only "viewing" actions (homepage, schedule list/details,
/// history): allowed for any logged-in user, or for anonymous visitors
/// when the "schedules are publicly visible" setting is on. Actions using
/// this must also be marked [AllowAnonymous] so the authorization
/// middleware lets the request through to this filter in the first place.
/// </summary>
public class ViewingAccessFilter : IAsyncAuthorizationFilter
{
    private readonly SettingsService _settingsService;

    public ViewingAccessFilter(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            return;
        }

        var settings = await _settingsService.GetAsync();
        if (!settings.SchedulesPubliclyVisible)
        {
            context.Result = new ChallengeResult();
        }
    }
}
