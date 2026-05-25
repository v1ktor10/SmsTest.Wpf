using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using SmsTest.Wpf.Models;
using SmsTest.Wpf.Services.AppSettings;
using SmsTest.Wpf.Services.CommentStorage;
using SmsTest.Wpf.Services.Environment;

namespace SmsTest.Wpf.ViewModels;

public sealed partial class MainViewModel(
    IEnvironmentService envService,
    ICommentStorageService commentStorage,
    IAppSettingsService settings,
    ILogger<MainViewModel> logger)
    : ObservableObject
{
    private readonly TimeSpan _userInputThrottle = TimeSpan.FromMilliseconds(250);

    [ObservableProperty] private ObservableCollection<EnvironmentVariableItemViewModel> _items = [];
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;
    private CancellationTokenSource? _saveCts;

    // public init (called from View.Loaded)
    public async Task InitializeAsync() => await LoadVariablesAsync();

    private async Task LoadVariablesAsync()
    {
        IsBusy = true;
        try
        {
            var comments = await commentStorage.LoadCommentsAsync();

            var names = settings.Current.EnvironmentVariableNames;
            var snapshot = new ObservableCollection<EnvironmentVariableItemViewModel>();

            foreach (var name in names)
            {
                var model = new EnvironmentVariableItem
                {
                    Name = name,
                    Value = envService.GetVariable(name) ?? string.Empty,
                    Comment = comments.TryGetValue(name, out var c) ? c : string.Empty
                };
                snapshot.Add(new EnvironmentVariableItemViewModel(model));
            }

            Items = snapshot;
            logger.LogInformation("Variables loaded: count={Count}", Items.Count);
            SetStatus($"Загружено переменных: {Items.Count}", isError: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load variables");
            SetStatus($"Ошибка загрузки: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetStatus(string message, bool isError) =>
        StatusMessage = isError ? $"{message}" : message;

    private async void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(EnvironmentVariableItemViewModel.Value)
            or nameof(EnvironmentVariableItemViewModel.Comment)))
            return;

        _saveCts?.Cancel();
        _saveCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(_userInputThrottle, _saveCts.Token);
            await SaveAsync();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var changed = Items.Where(i => i.HasValueChanges).ToList();

            foreach (var item in changed)
            {
                var old = item.CommittedValue;
                envService.SetVariable(item.Name, item.Value);
                item.Commit();

                logger.LogInformation(
                    "ENV CHANGED | Name={Name} | OldValue={OldValue} | NewValue={NewValue}",
                    item.Name, old, item.Value);
            }

            var comments = Items.ToDictionary(i => i.Name, i => i.Comment);
            await commentStorage.SaveCommentsAsync(comments);

            logger.LogInformation("Auto-save completed at: {DateTime:HH:mm:ss}", DateTime.Now);
            SetStatus($"Сохранено в {DateTime.Now:HH:mm:ss}", isError: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Auto-save failed");
            SetStatus($"Ошибка сохранения: {ex.Message}", isError: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnItemsChanging(ObservableCollection<EnvironmentVariableItemViewModel>? oldValue,
        ObservableCollection<EnvironmentVariableItemViewModel> newValue)
    {
        if (oldValue is not null)
            foreach (var item in oldValue)
                item.PropertyChanged -= OnItemPropertyChanged;

        foreach (var item in newValue)
            item.PropertyChanged += OnItemPropertyChanged;
    }
}