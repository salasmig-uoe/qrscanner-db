
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
    EditTransactionsPage _editTransactionsPage;
    public SellingRecordPage(LocalDbService dbService, MainViewModel vm, PopupResult result, EmailViewModel emailViewModel, EditTransactionsPage editTransactionsPage)	
    {
		InitializeComponent();
        _dbService = dbService;
        _vm = vm;
        BindingContext = vm;
        _result = result;
        _emailViewModel = emailViewModel;
        _editTransactionsPage = editTransactionsPage;
    }

    private async void OnSellingButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.DetailViewPage(_dbService, _vm,_result));
    }

    private async void OnEmailButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.EmailFormPage(_emailViewModel)); 
    }

    private async void OnEditTransferButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.EditTransactionsPage(_dbService, _vm));
    }

}