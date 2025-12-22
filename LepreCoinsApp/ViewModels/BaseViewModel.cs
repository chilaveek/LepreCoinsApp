namespace LepreCoinsApp.ViewModels;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;
    [ObservableProperty]
    private bool _isBusy;
    [ObservableProperty]
    private string _busyText = string.Empty;
}