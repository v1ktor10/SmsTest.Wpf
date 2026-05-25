using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SmsTest.Wpf.Services.AppSettings;

namespace SmsTest.Wpf.Services.CommentStorage;

/// <inheritdoc />
public sealed class CommentStorageService : ICommentStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly ILogger<CommentStorageService> _logger;

    public CommentStorageService(IAppSettingsService settings, ILogger<CommentStorageService> logger)
    {
        _logger = logger;
        _filePath = Path.Combine(AppContext.BaseDirectory, settings.Current.CommentsFileName);
        _logger.LogDebug("CommentStorageService: file={Path}", _filePath);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, string>> LoadCommentsAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogDebug("Comments file not found, returning empty dictionary");
            return new Dictionary<string, string>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath, ct);
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
            _logger.LogDebug("Loaded {Count} comments from {Path}", result.Count, _filePath);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load comments from {Path}", _filePath);
            return new Dictionary<string, string>();
        }
    }

    /// <inheritdoc/>
    public async Task SaveCommentsAsync(IDictionary<string, string> comments, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(comments, JsonOptions);
            await File.WriteAllTextAsync(_filePath, json, ct);
            _logger.LogDebug("Saved {Count} comments to {Path}", comments.Count, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save comments to {Path}", _filePath);
            throw;
        }
    }
}