using CommunityToolkit.Mvvm.ComponentModel;

namespace SmsTest.Wpf.Models;

public sealed record EnvironmentVariableItem
{
    public string Name { get; init; } = string.Empty;
    
    public string Value { get; set; } = string.Empty;
    
    public string Comment { get; set; } = string.Empty;
}