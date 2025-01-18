

namespace QRScanner.Pages;

public partial class SellingRecordPage : ContentPage
{
    int _capturedItem = 0;
	public SellingRecordPage()
	{
		InitializeComponent();
        codeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
        {
            Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = true
        };
        codeReader.IsEnabled = true;
    }
    //new BarcodeScannerBindingContext BindingContext => (BarcodeScannerBindingContext)base.BindingContext;
    private void OnBarcodeDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        /*
        BindingContext.BarcodeLabelText = $"{e.Results.FirstOrDefault()?.Value}";
        BindingContext.IsDetectingInternal = false;
        */
        //Vibration.Vibrate();

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
    }
}