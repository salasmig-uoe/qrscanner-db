
using CommunityToolkit.Maui.Views;
using QRScanner.Popups;
using QRScanner.ViewModel;

namespace QRScanner.Pages;

public partial class SellingRecordPage : ContentPage
{
    int _capturedItem = 0;
    private MainViewModel VM;

    private PopupResult _result;
    public SellingRecordPage(MainViewModel vm, PopupResult result)	
    {
		InitializeComponent();

        VM = vm;
        _result = result;
        BindingContext = VM;
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync(new PopupPage(VM,_result));
        if (result != null)
        {
            PopupResult res = (PopupResult)result;
            VM.BarcodeLabelText = res.ReturnData;
            VM.Text = res.ReturnData;
        }
    }

}