
using ZXing.Net.Maui;

namespace QRScanner.Pages
{
    public partial class CameraPopupPage : ContentPage
    {
        public event Action<string> OnQRCodeDetected;

        // Define the TaskCompletionSource to handle the QR code result
        public TaskCompletionSource<string> QRCodeTaskCompletionSource { get; } = new TaskCompletionSource<string>();

        public CameraPopupPage()
        {
            InitializeComponent();
        }

        private void OnBarcodeDetectedx(object sender, BarcodeDetectionEventArgs e)
        {
            if (e.Results.Length > 0)
            {
                var qrCodeValue = e.Results[0].Value;
                OnQRCodeDetected?.Invoke(qrCodeValue);

                // Close the popup
                Navigation.PopModalAsync();
            }
        }


        private void OnBarcodeDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (e.Results.Length > 0)
            {
                var qrCodeValue = e.Results[0].Value;

                // Set the result of the TaskCompletionSource
                QRCodeTaskCompletionSource.TrySetResult(qrCodeValue);

                // Close the popup on the UI thread
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopModalAsync();
                });
            }
        }
    }
}