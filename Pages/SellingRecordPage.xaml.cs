
using QRScanner.Pages;
using QRScanner.Popups;
using QRScanner.Services;
using QRScanner.ViewModel;

namespace QRScanner.Pages;

public partial class SellingRecordPage : ContentPage
{
    LocalDbService _dbService;
    MainViewModel _vm;
    PopupResult _result;
    EmailViewModel _emailViewModel;
    public SellingRecordPage(LocalDbService dbService, MainViewModel vm, PopupResult result, EmailViewModel emailViewModel)	
    {
		InitializeComponent();
        _dbService = dbService;
        _vm = vm;
        BindingContext = vm;
        _result = result;
        _emailViewModel = emailViewModel;
    }

    private async void OnSellingButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.DetailViewPage(_dbService, _vm,_result));
    }

    private async void OnEmailButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.EmailFormPage(_emailViewModel)); 
    }

}