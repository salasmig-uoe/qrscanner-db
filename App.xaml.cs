namespace QRScanner
{
    public partial class App : Application
    {
        public App(MainPage mainPage)
        {
            InitializeComponent();
            //MainPage = mainPage;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}