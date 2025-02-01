using QRScanner.Services;
using QRScanner.ViewModel;
namespace QRScanner
{
    public partial class App : Application
    {
        public App(LocalDbService dbService, MainViewModel vm)
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}