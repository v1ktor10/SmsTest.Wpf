using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmsTest.Wpf.Services.AppSettings;

namespace SmsTest.Wpf.Services.Environment;

/// <summary>
/// Читает и записывает переменные среды Windows через <see cref="Environment"/>.
/// </summary>
public sealed class EnvironmentService : IEnvironmentService
{
    private readonly ILogger<EnvironmentService> _logger;
    private readonly EnvironmentVariableTarget _target;

    public EnvironmentService(IAppSettingsService settings, ILogger<EnvironmentService> logger)
    {
        _logger = logger;

        _target = Enum.TryParse<EnvironmentVariableTarget>(
            settings.Current.EnvironmentVariableTarget,
            ignoreCase: true,
            out var t)
            ? t
            : EnvironmentVariableTarget.User;

        _logger.LogDebug("EnvironmentService initialised with target={Target}", _target);
    }

    /// <inheritdoc/>
    public string? GetVariable(string name)
    {
        var value = System.Environment.GetEnvironmentVariable(name, _target);
        _logger.LogDebug("GetVariable: {Name} = {Value}", name, value ?? "<null>");
        return value;
    }

    /// <inheritdoc/>
    public void SetVariable(string name, string value)
    {
        System.Environment.SetEnvironmentVariable(name, value, _target);
        _logger.LogDebug("SetVariable: {Name} = {Value}", name, value);
    }
}