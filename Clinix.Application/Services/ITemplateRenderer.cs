

namespace Clinix.Application.Services;

/// <summary>
/// Renders message templates given a template body and a model.
/// Implementations should be safe (no arbitrary code execution) and deterministic.
/// </summary>
public interface ITemplateRenderer
    {
    /// <summary>
    /// Renders a template body with the provided model.
    /// Model is an arbitrary dictionary of key->value pairs.
    /// </summary>
    string Render(string templateBody, IDictionary<string, object?> model);
    }

