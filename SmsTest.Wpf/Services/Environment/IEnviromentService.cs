namespace SmsTest.Wpf.Services;

/// <summary>
/// Абстракция над System.Environment для чтения / записи переменных среды.
/// </summary>
public interface IEnvironmentService
{
    /// <summary>Возвращает значение переменной или <c>null</c>, если она не задана.</summary>
    string? GetVariable(string name);

    /// <summary>Записывает значение переменной с заданным таргетом.</summary>
    void SetVariable(string name, string value);
}