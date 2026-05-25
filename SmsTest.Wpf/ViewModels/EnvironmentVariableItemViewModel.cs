using CommunityToolkit.Mvvm.ComponentModel;
using SmsTest.Wpf.Models;

namespace SmsTest.Wpf.ViewModels;

public sealed partial class EnvironmentVariableItemViewModel(EnvironmentVariableItem model) : ObservableObject
{
    public string Name => model.Name;

    [ObservableProperty] 
    private string _value = model.Value;

    [ObservableProperty] 
    private string _comment = model.Comment;

    public string CommittedValue { get; private set; } = model.Value;
    public bool HasValueChanges => Value != CommittedValue;
    
    public void Commit()
    {
        model.Value   = Value;
        model.Comment = Comment;
        CommittedValue = Value;
    }
}