using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SmsTest.Wpf.Services.CommentStorage;

/// <summary>
/// Хранит комментарии в JSON-файле рядом с исполняемым файлом приложения.
/// Файл: comments.json
/// </summary>
public sealed class CommentStorageService : ICommentStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly ILogger<CommentStorageService> _logger;

    public CommentStorageService(ILogger<CommentStorageService> logger)
    {
        _logger = logger;

        var baseDir = AppContext.BaseDirectory;
        _filePath = Path.Combine(baseDir, "comments.json");
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> LoadComments()
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogDebug("Comments file not found, returning empty dictionary");
            return new Dictionary<string, string>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            _logger.LogDebug("Loaded {Count} comments from {Path}", dict.Count, _filePath);
            return dict;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load comments from {Path}, returning empty dictionary", _filePath);
            return new Dictionary<string, string>();
        }
    }

    /// <inheritdoc/>
    public void SaveComments(IDictionary<string, string> comments)
    {
        try
        {
            var json = JsonSerializer.Serialize(comments, JsonOptions);
            File.WriteAllText(_filePath, json);
            _logger.LogDebug("Saved {Count} comments to {Path}", comments.Count, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save comments to {Path}", _filePath);
            throw;
        }
    }
}