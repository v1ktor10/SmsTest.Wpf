namespace SmsTest.Wpf.Models;

public sealed record AppSettings
{
    public const string SectionName = "App";

    /// <summary>Имена переменных среды, отображаемых в таблице</summary>
    public string[] EnvironmentVariableNames { get; init; } = [];

    /// <summary>Таргет Windows-реестра: "User" или "Machine" (Machine требует прав администратора)</summary>
    public string EnvironmentVariableTarget { get; init; } = "User";

    /// <summary>Имя файла для хранения пользовательских комментариев.</summary>
    public string CommentsFileName { get; init; } = "comments.json";

    /// <summary>Папка для лог-файлов относительно каталога исполняемого файла.</summary>
    public string LogDirectory { get; init; } = "logs";

    /// <summary>Количество хранимых суточных лог-файлов.</summary>
    public int RetainedLogFileCount { get; init; } = 30;
}