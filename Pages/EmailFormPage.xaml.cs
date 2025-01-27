using QRScanner.ViewModel;
namespace QRScanner.Pages;

public partial class EmailFormPage : ContentPage
{
    public EmailFormPage(EmailViewModel emailViewModel)
    {
        InitializeComponent();
        BindingContext = emailViewModel;
    }


    public void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        // Handle the date selected event

    }

}