

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
        VM.IsDetectingInternal = true;
        VM.IsDetecting = true;
        // Check camera permission
        CheckCameraPermission();

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

    private void OnBarcodeDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        
        
        VM.BarcodeLabelText = $"{e.Results.FirstOrDefault()?.Value}";
        VM.IsDetectingInternal = false;

        Dispatcher.DispatchAsync(async () =>
        {
            await DisplayAlert("Barcode Detected", VM.BarcodeLabelText, "OK");
        });
        

        //Vibration.Vibrate();

        /*
        var first = e.Results?.FirstOrDefault();
        if (first is null)
            return;
                       
        // It is waiting to capture the following Item
        if (_capturedItem == 0)
        {
            _capturedItem = 1;
            //analyseContent(sender, first.Value);
            String strcode = first.Value;
            //analyseContent(strcode);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                item_codeEntryField.Text = "Item";
            });
            codeReader.IsEnabled = false;
            codeReader.IsDetecting = false;
            codeReader.IsVisible = false;
            Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("Barcode Detected", first.Value, "OK");
            });
        }
        */
    }
}