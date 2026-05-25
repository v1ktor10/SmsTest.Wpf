namespace SmsTest.Wpf.Services.CommentStorage;

/// <summary>
/// Сервис для работы с комментариями
/// </summary>
public interface ICommentStorageService
{
    /// <summary>Загружает словарь имя → комментарий.</summary>
    Task<IReadOnlyDictionary<string, string>> LoadCommentsAsync(CancellationToken ct = default);

    /// <summary>Сохраняет словарь имя → комментарий.</summary>
    Task SaveCommentsAsync(IDictionary<string, string> comments, CancellationToken ct = default);
}
