using QRScanner.Services;
using QRScanner.ViewModel;
using QRScanner.Pages;
using QRScanner.Popups;
namespace QRScanner
{
    public partial class App : Application
    {
        public App(LocalDbService dbService, MainViewModel vm, PopupResult result)
        {
            InitializeComponent();
            //MainPage = new NavigationPage(new DetailViewPage(dbService, vm, result));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}