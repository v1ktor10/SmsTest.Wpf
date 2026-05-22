using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmsTest.Wpf.Models;
using SmsTest.Wpf.Services;

namespace SmsTest.Wpf.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    // ── dependencies ──────────────────────────────────────────────────────
    private readonly IEnvironmentService      _envService;
    private readonly ICommentStorageService   _commentStorage;
    private readonly ILogger<MainViewModel>   _logger;
    private readonly IReadOnlyList<string>    _variableNames;

    // ── observable state ──────────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<EnvironmentVariableItem> _items = [];

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    // ── ctor ──────────────────────────────────────────────────────────────
    public MainViewModel(
        IEnvironmentService    envService,
        ICommentStorageService commentStorage,
        ILogger<MainViewModel> logger,
        IConfiguration         configuration)
    {
        _envService     = envService;
        _commentStorage = commentStorage;
        _logger         = logger;
        _variableNames  = configuration
            .GetSection("EnvironmentVariables:Names")
            .Get<string[]>() ?? [];

        LoadVariables();
    }

    // ── commands ──────────────────────────────────────────────────────────

    [RelayCommand]
    private void Refresh() => LoadVariables();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        IsBusy = true;
        try
        {
            var changed = Items.Where(i => i.HasValueChanges).ToList();

            foreach (var item in changed)
            {
                var old = item.CommittedValue;
                _envService.SetVariable(item.Name, item.Value);
                item.CommittedValue = item.Value;

                _logger.LogInformation(
                    "ENV CHANGED | Name={Name} | OldValue={OldValue} | NewValue={NewValue}",
                    item.Name, old, item.Value);
            }

            // Всегда сохраняем комментарии (могли измениться без изменения значений).
            var comments = Items.ToDictionary(i => i.Name, i => i.Comment);
            _commentStorage.SaveComments(comments);

            var msg = changed.Count > 0
                ? $"Сохранено изменений: {changed.Count}"
                : "Комментарии обновлены, значения без изменений";

            _logger.LogInformation("Save completed: {Message}", msg);
            SetStatus(msg, isError: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save failed");
            SetStatus($"Ошибка сохранения: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() => !IsBusy;

    // ── private helpers ───────────────────────────────────────────────────

    private void LoadVariables()
    {
        IsBusy = true;
        try
        {
            var comments = _commentStorage.LoadComments();
            var snapshot = new ObservableCollection<EnvironmentVariableItem>();

            foreach (var name in _variableNames)
            {
                var value = _envService.GetVariable(name) ?? string.Empty;

                snapshot.Add(new EnvironmentVariableItem
                {
                    Name           = name,
                    Value          = value,
                    CommittedValue = value,
                    Comment        = comments.TryGetValue(name, out var c) ? c : string.Empty
                });
            }

            Items = snapshot;
            _logger.LogInformation("Variables loaded: count={Count}", Items.Count);
            SetStatus($"Загружено переменных: {Items.Count}", isError: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load variables");
            SetStatus($"Ошибка загрузки: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = isError ? $"⚠ {message}" : message;
    }
}