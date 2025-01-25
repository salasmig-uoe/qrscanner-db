using QRScanner.ViewModel;
namespace QRScanner.Pages;

public partial class EmailFormPage : ContentPage
{
    public EmailFormPage(EmailViewModel emailViewModel)
    {
        InitializeComponent();
        BindingContext = emailViewModel;
    }
}