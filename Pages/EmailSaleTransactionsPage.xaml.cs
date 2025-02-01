using QRScanner.ViewModel;
namespace QRScanner.Pages;

public partial class EmailSaleTransactionsPage : ContentPage
{
    public EmailSaleTransactionsPage(EmailViewModel emailViewModel)
    {
        InitializeComponent();
        BindingContext = emailViewModel;
    }


    public void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        // TODO: Handle the date selected event
    }
    private async void OnGoBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

}