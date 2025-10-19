using System.Text.RegularExpressions;
using Clinix.Application.Services;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;

namespace Clinix.Infrastructure.Background;

public class HandlebarsTemplateRenderer : ITemplateRenderer
    {
    private readonly ILogger<HandlebarsTemplateRenderer> _logger;
    private static readonly Regex ScriptTagRegex = new("<script.*?>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

    public HandlebarsTemplateRenderer(ILogger<HandlebarsTemplateRenderer> logger)
        {
        _logger = logger;
        }

    public string Render(string template, object model)
        {
        try
            {
            template = ScriptTagRegex.Replace(template, string.Empty);
            var compiled = Handlebars.Compile(template);
            return compiled(model);
            }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Template rendering failed");
            return string.Empty;
            }
        }

    string ITemplateRenderer.Render(string templateBody, IDictionary<string, object?> model)
        {
        throw new NotImplementedException();
        }
    }
