// Clinix.Web/Extensions/NavigationManagerExtensions.cs
using System.Web;
using Microsoft.AspNetCore.Components;

namespace Clinix.Web.Extensions;

public static class NavigationManagerExtensions
    {
    /// <summary>
    /// Attempts to retrieve a specified query string value from the current URI.
    /// </summary>
    public static bool TryGetQueryString(this NavigationManager navigationManager, string key, out string value)
        {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);

        // Use HttpUtility.ParseQueryString to parse the query parameters
        var query = HttpUtility.ParseQueryString(uri.Query);

        value = query.Get(key);

        return !string.IsNullOrEmpty(value);
        }
    }
