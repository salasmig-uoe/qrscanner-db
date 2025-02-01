using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using QRScanner.Pages;
using QRScanner.Services;
using QRScanner.ViewModel;
using ZXing.Net.Maui.Controls;

namespace QRScanner
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }).UseBarcodeReader();

            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<AlertService>();
            builder.Services.AddSingleton<EmailViewModel>();
            builder.Services.AddSingleton<EmailSaleTransactionsPage>();
            builder.Services.AddSingleton<EditSaleTransactionsPage>();
            builder.Services.AddSingleton<CameraPopupPage>();
            builder.Services.AddSingleton<CreateSaleTransactionsPage>();
            builder.Services.AddSingleton<ViewSaleTransactionsPage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
