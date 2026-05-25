using Microsoft.Extensions.Options;

namespace SmsTest.Wpf.Services.AppSettings;

/// <summary>
/// Реализация <see cref="IAppSettingsService"/> поверх стандартного <see cref="IOptions{TOptions}"/>.
/// </summary>
public sealed class AppSettingsService(IOptions<Models.AppSettings> options) : IAppSettingsService
{
    public Models.AppSettings Current { get; } = options.Value;
}