
using CommunityToolkit.Maui.Views;
using QRScanner.ViewModel;

namespace QRScanner.Pages;

public partial class SellingRecordPage : ContentPage
{
    int _capturedItem = 0;
    private MainViewModel VM;
    public SellingRecordPage(MainViewModel vm)
	{
		InitializeComponent();
        codeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
        {
            Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = true,
            TryHarder = true,
        };
        VM = vm;
        BindingContext = vm;
        VM.IsDetectingInternal = false;
        VM.IsDetecting = true;

        // Check camera permission
        CheckCameraPermission();

        // Add Unloaded event handler
        Unloaded += OnPageUnloaded;

    }
    //new BarcodeScannerBindingContext BindingContext => (BarcodeScannerBindingContext)base.BindingContext;


    private async Task CheckCameraPermission()
    {
        codeReader.IsEnabled = false;
        while (true)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status == PermissionStatus.Granted)
            {
                codeReader.IsEnabled = true;
                break;
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }


    private void analyseContent(String strcode)
    {
        String[] fields = strcode.Split(':');
        MainThread.BeginInvokeOnMainThread(() =>
        {

            Dictionary<string,string> field_key = new Dictionary<string, string>(8);
            String full_text = "";
            for (int i = 0; i < fields.Length; i++)
            {
                String field_value = fields[i].Trim();
                switch (i)
                {
                    case 0:
                        //artist_codeEntryField.Text = field_value;
                        field_key.Add("artist_code", field_value);
                        break;
                    case 1:
                        //titleEntryField.Text = field_value;
                        //amountEntryField.Text = field_value;
                        field_key.Add("title", field_value);
                        full_text = "Title:   " + field_value + "    ";
                        break;
                    case 2:
                        //typeEntryField.Text = field_value;
                        field_key.Add("work_type", field_value);
                        full_text += "Media:    " + field_value + "     ";
                        break;
                    case 3:
                        field_key.Add("size", field_value);
                        full_text += "Dimensions:    " + field_value + " (cm)";
                        break;
                    case 4:
                        field_key.Add("price", field_value);
                        //priceEntryField.Text = field_value;
                        break;
                    case 5:
                        field_key.Add("amount", field_value);
                        break;
                    case 6:
                        field_key.Add("item_code", field_value);
                        //item_codeEntryField.Text = field_value;
                        //transaction_typeEntryField.Text = field_value;
                        break;
                    case 7:
                        field_key.Add("qr_id", field_value);
                        break;
                }
            }
            int finished = 1;
            //amountEntryField.Text = full_text;
        });
    }
    private void OnBarcodeDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {

        var first = e.Results?.FirstOrDefault();
        if (first is null)
            return;

        VM.BarcodeLabelText = $"{first.Value}";
        VM.IsDetectingInternal = false;

        String strcode = first.Value;
        analyseContent(strcode);


        Dispatcher.DispatchAsync(async () =>
        {
            await DisplayAlert("Barcode Detected", VM.BarcodeLabelText, "OK");
        });
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        this.ShowPopup(new PopupPage(VM));
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        /*
        // Show the popup
        var result = await this.ShowPopupAsync(new QRScanner.Popups.CodeReaderPopup());
        if (result is string barcode)
        {
            analyseContent(barcode);
            await DisplayAlert("Barcode Detected", barcode, "OK");
        }
        */
        codeReader.IsDetecting = true;
        // Re-enable the codeReader if permissions are granted
        CheckCameraPermission();

    }

    // Unloaded event handler
    private void OnPageUnloaded(object sender, EventArgs e)
    {
        VM.IsDetectingInternal = false;
        codeReader.Handler?.DisconnectHandler();
    }
}