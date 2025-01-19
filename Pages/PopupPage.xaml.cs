using CommunityToolkit.Maui.Views;
using QRScanner.Popups;
using QRScanner.ViewModel;

namespace QRScanner.Pages;

public partial class PopupPage: Popup
{
    private MainViewModel VM;
    private PopupResult _result;
    public PopupPage(MainViewModel vm, PopupResult result)
	{
		InitializeComponent();

        _result = result;

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

        CheckCameraPermission();
    }
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
            Dictionary<string, string> field_key = new Dictionary<string, string>(8);
            String full_text = "";
            for (int i = 0; i < fields.Length; i++)
            {
                String field_value = fields[i].Trim();
                switch (i)
                {
                    case 0:
                        field_key.Add("artist_code", field_value);
                        break;
                    case 1:
                        field_key.Add("title", field_value);
                        full_text = "Title:   " + field_value + "    ";
                        break;
                    case 2:
                        field_key.Add("work_type", field_value);
                        full_text += "Media:    " + field_value + "     ";
                        break;
                    case 3:
                        field_key.Add("size", field_value);
                        full_text += "Dimensions:    " + field_value + " (cm)";
                        break;
                    case 4:
                        field_key.Add("price", field_value);
                        break;
                    case 5:
                        field_key.Add("amount", field_value);
                        break;
                    case 6:
                        field_key.Add("item_code", field_value);
                        break;
                    case 7:
                        field_key.Add("qr_id", field_value);
                        break;
                }
            }


            _result.ReturnData =field_key["item_code"];
            this.Close(_result);
            //Dismiss(_result); // Ensure this line is active

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
    }


}