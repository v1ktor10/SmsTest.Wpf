namespace SmsTest.Wpf.Services.AppSettings;

/// <summary>
/// Предоставляет доступ к актуальным настройкам приложения.
/// </summary>
public interface IAppSettingsService
{
    Models.AppSettings Current { get; }
}
