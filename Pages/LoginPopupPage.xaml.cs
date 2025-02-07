using Microsoft.Maui.Controls;
namespace QRScanner.Pages;

public partial class LoginPopupPage : ContentPage
{
    public LoginPopupPage()
    {
        InitializeComponent();
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        // Validate input (optional)
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please enter both email and password.", "OK");
            return;
        }

        // Pass the values back to the calling page
        await Navigation.PopAsync(); // Close the popup
        MessagingCenter.Send(this, "LoginData", new Tuple<string, string>(email, password));
    }
}


