// SafeNavigationService.cs
using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Clinix.Web.Helpers;
public interface ISafeNavigationService
    {
    Task NavigateAsync(string uri, bool forceLoad = false, bool useReplace = false);
    }

public class SafeNavigationService : ISafeNavigationService
    {
    private readonly NavigationManager _nav;
    private readonly IJSRuntime _js;
    private readonly ILogger<SafeNavigationService> _logger;

    public SafeNavigationService(NavigationManager nav, IJSRuntime js, ILogger<SafeNavigationService> logger)
        {
        _nav = nav;
        _js = js;
        _logger = logger;
        }

    public async Task NavigateAsync(string uri, bool forceLoad = false, bool useReplace = false)
        {
        if (string.IsNullOrWhiteSpace(uri))
            {
            _logger.LogWarning("NavigateAsync called with empty uri");
            return;
            }

        try
            {
            _nav.NavigateTo(uri, forceLoad);
            return;
            }
        catch (Exception ex)
            {
            _logger.LogWarning(ex, "NavigationManager.NavigateTo threw for {Uri}", uri);
            }

        try
            {
            // try full reload
            _nav.NavigateTo(uri, forceLoad: true);
            return;
            }
        catch (Exception ex)
            {
            _logger.LogWarning(ex, "ForceLoad NavigateTo failed for {Uri}", uri);
            }

        try
            {
            await _js.InvokeVoidAsync("blazorHelpers.forceRedirect", uri, useReplace);
            return;
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "JS fallback failed for {Uri}", uri);
            }

        _logger.LogError("All navigation methods failed for {Uri}", uri);
        }
    }
