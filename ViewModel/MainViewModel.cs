using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRScanner.Services;
namespace QRScanner.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly AlertService _alertService;


    [ObservableProperty]
    decimal totalAmount;

    [ObservableProperty]
    decimal cashAmount;

    [ObservableProperty]
    decimal cardAmount;

    [ObservableProperty]
    decimal donationAmount;

    public MainViewModel(AlertService alertService)
    {
        _alertService = alertService;
        Items = new ObservableCollection<string>();
    }

    [ObservableProperty]
    ObservableCollection<string> items;

    [ObservableProperty]
    string text;

    [RelayCommand]
    void Add()
    {
        if (string.IsNullOrEmpty(Text))
            return;

        Items.Add(Text);

        // Add our item
        Text = string.Empty;
    }

    [RelayCommand]
    async Task DeleteAsync(string text)
    {
        if (Items.Contains(text))
        {
            bool result = await _alertService.ShowConfirmationAsync("Confirm Delete", $"Do you want to delete the item '{text}'?", "OK", "Cancel");
            if (result)
            {
                Items.Remove(text);
                await _alertService.ShowMessageAsync("Deleted", $"'{text}' has been deleted.", "OK");
            }
            else
            {
                await _alertService.ShowMessageAsync("Operation Canceled", "", "OK");
            }
        }
    }
    //=========== SCANNING ==============
    [ObservableProperty]
    public string barcodeLabelText= "|";

    [ObservableProperty]
    bool useExternal = false;

    [ObservableProperty]
    bool isDetectingExternal = default;

    [ObservableProperty]
    bool isDetectingInternal = false;

    [ObservableProperty]
    bool isDetecting = false;

    [RelayCommand]
    void Scan()
    {
        if (UseExternal)
        {
            IsDetectingExternal = true;
        }
        else
        {
            IsDetectingInternal = true;
        }
    }

    [RelayCommand]
    void Cancel()
    {
        IsDetectingInternal = IsDetectingExternal = false;
    }
}

