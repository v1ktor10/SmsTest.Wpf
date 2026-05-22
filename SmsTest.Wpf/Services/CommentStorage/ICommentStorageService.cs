namespace SmsTest.Wpf.Services;

/// <summary>
/// Хранит пользовательские комментарии к переменным (env vars не поддерживают метаданные нативно).
/// </summary>
public interface ICommentStorageService
{
    /// <summary>Загружает словарь имя→комментарий</summary>
    IReadOnlyDictionary<string, string> LoadComments();

    /// <summary>Сохраняет словарь имя→комментарий</summary>
    void SaveComments(IDictionary<string, string> comments);
}