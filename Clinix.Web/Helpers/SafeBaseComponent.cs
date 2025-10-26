
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Clinix.Web.Helpers;
public abstract class SafeBaseComponent : ComponentBase, IDisposable
    {
    [Inject] protected NavigationManager Navigation { get; private set; } = default!;
    [Inject] protected IJSRuntime JS { get; private set; } = default!;
    [Inject] protected ILogger<SafeBaseComponent> Logger { get; private set; } = default!;

    private bool _disposed;

    /// <summary>
    /// Safely navigate to the given URI. Retries using InvokeAsync, then falls back to full page load and JS redirect.
    /// Use this from event handlers or lifecycle methods (not in ctor).
    /// </summary>
    protected async Task SafeNavigateAsync(string uri, bool forceLoad = false, bool useReplace = false)
        {
        if (_disposed)
            {
            Logger.LogWarning("Component disposed; skipping navigation to {Uri}", uri);
            return;
            }

        if (string.IsNullOrWhiteSpace(uri))
            {
            Logger.LogWarning("SafeNavigateAsync called with empty uri");
            return;
            }

        // 1) Try normal NavigateTo (fast, keeps SPA)
        try
            {
            Navigation.NavigateTo(uri, forceLoad);
            return;
            }
        catch (Exception ex)
            {
            Logger.LogWarning(ex, "NavigateTo threw on first attempt to {Uri}", uri);
            }

        // 2) Retry via InvokeAsync (avoid "during render" issues)
        try
            {
            await InvokeAsync(async () =>
            {
                try
                    {
                    await Task.Delay(100); // give Blazor a tick to settle
                    Navigation.NavigateTo(uri, forceLoad);
                    }
                catch (Exception inner)
                    {
                    // bubble to outer catch
                    throw new InvalidOperationException("InvokeAsync retry failed", inner);
                    }
            });
            return;
            }
        catch (Exception ex)
            {
            Logger.LogWarning(ex, "InvokeAsync retry failed for {Uri}", uri);
            }

        // 3) Force full page reload (helps cookie visibility / server-side auth)
        try
            {
            Navigation.NavigateTo(uri, forceLoad: true);
            return;
            }
        catch (Exception ex)
            {
            Logger.LogWarning(ex, "ForceLoad NavigateTo failed for {Uri}", uri);
            }

        // 4) Final fallback: client-side JS redirect
        try
            {
            await JS.InvokeVoidAsync("blazorHelpers.forceRedirect", uri, useReplace);
            return;
            }
        catch (Exception ex)
            {
            Logger.LogError(ex, "JS fallback redirect failed for {Uri}", uri);
            }

        Logger.LogError("All navigation fallbacks failed for {Uri}", uri);
        }

    protected virtual void Dispose(bool disposing)
        {
        _disposed = true;
        }

    public void Dispose()
        {
        Dispose(true);
        GC.SuppressFinalize(this);
        }
    }
