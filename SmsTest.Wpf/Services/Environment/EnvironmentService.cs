using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SmsTest.Wpf.Services;

/// <summary>
/// Читает и записывает переменные среды Windows через <see cref="Environment"/>.
/// Таргет конфигурируется через appsettings.json → EnvironmentVariables:Target.
/// </summary>
public sealed class EnvironmentService : IEnvironmentService
{
    private readonly ILogger<EnvironmentService> _logger;
    private readonly EnvironmentVariableTarget _target;

    public EnvironmentService(IConfiguration configuration, ILogger<EnvironmentService> logger)
    {
        _logger = logger;

        var raw = configuration["EnvironmentVariables:Target"] ?? "User";
        _target = Enum.TryParse<EnvironmentVariableTarget>(raw, ignoreCase: true, out var t)
            ? t
            : EnvironmentVariableTarget.User;

        _logger.LogDebug("EnvironmentService initialised with target={Target}", _target);
    }

    /// <inheritdoc/>
    public string? GetVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name, _target);
        _logger.LogDebug("GetVariable: {Name} = {Value}", name, value ?? "<null>");
        return value;
    }

    /// <inheritdoc/>
    public void SetVariable(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value, _target);
        _logger.LogDebug("SetVariable: {Name} = {Value}", name, value);
    }
}