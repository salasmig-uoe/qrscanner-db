using QRScanner.Services;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Text;


namespace QRScanner.Pages;

public partial class EmailSaleTransactionsPage : ContentPage
{
    private LocalDbService _dbService;
    private AlertService _alertService;
    private string _email_provider = "gmail.com";

    // Event to return the user input
    public event Action<string, string, DateTime> OnSubmit;
    public EmailSaleTransactionsPage(AlertService alertService, LocalDbService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
        _alertService = alertService;
    }
    private async Task<List<decimal>> Get_stats_transactionAsync(string formattedDate)
    {
        var items = await _dbService.GetPaymentTransferByDate(formattedDate);
        decimal cash = 0;
        decimal card = 0;
        decimal count = 0;
        foreach (var item in items)
        {
            count += 1;
            if (item.TransactionType.ToUpper() == "CASH")
            {
                cash += (decimal)item.Amount;
            }
            else if (item.TransactionType.ToUpper() == "CARD")
            {
                card += (decimal)item.Amount;
            }
        }
        return new List<decimal> { count, cash, card };
    }
    private async Task<string> Get_day_transactionAsync(string formattedDate)
    {
        var items = await _dbService.GetPaymentTransferByDate(formattedDate);
        string result = "";
        foreach (var item in items)
            result += item.PaymentId + ";" + item.TransactionCode + ";" + item.ItemCode + ";" + item.Quantity + ";" + item.Amount + ";" + item.TransactionType + ";" + item.Created + ";" + item.Updated + "\n";
        return result;
    }


    private async Task OnEmailButtonClicked(string username, string password, DateTime date)
    {
        string day_of_sale = date.ToString("D");
        string date_to_query = date.ToString("yyyy-MM-dd");

        // Email details
        string fromEmail = $"{username}@{_email_provider}";
        string toEmail = $"{username}@yahoo.com.mx, {username}.cees@{_email_provider}";
        string subject = $"Sales report: {day_of_sale}";
        string body = "Email sent from a QRScanner app";

        // Gmail SMTP settings
        string smtpHost = $"smtp.{_email_provider}";
        int smtpPort = 587;
        string smtpUsername = $"{username}@{_email_provider}";
        string smtpPassword = $"{password}";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await _alertService.ShowMessageAsync("Error", "Please enter your email and password", "OK");
            return;
        }

        // Create the email message
        MailMessage mail = new MailMessage(fromEmail, toEmail, subject, body);

        // Getting all the transactions for the day

        List<decimal> stats = await Get_stats_transactionAsync(date_to_query);
        string transactions = await Get_day_transactionAsync(date_to_query);

        string total = $"{stats[1] + stats[2]:C2}";
        string alert_message = $" {(int)stats[0]} Transactions for a total of {total} on the {day_of_sale}";

        bool result = await _alertService.ShowConfirmationAsync("Confirm the information is correct", alert_message, "OK", "Cancel");
        if (result)
        {
            // String to be attached as a text file
            string attachmentContent = transactions;
            string attachmentFileName = $"{date_to_query}.csv";

            // Convert the string to a MemoryStream
            byte[] attachmentBytes = Encoding.UTF8.GetBytes(attachmentContent);
            MemoryStream attachmentStream = new MemoryStream(attachmentBytes);

            // Create the attachment
            Attachment attachment = new Attachment(attachmentStream, attachmentFileName, MediaTypeNames.Text.Plain);
            mail.Attachments.Add(attachment);

            // Set up the SMTP client
            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            try
            {
                // Send the email
                smtpClient.Send(mail);
                // Update the status label
                await Application.Current.MainPage.DisplayAlert("Status: Email sent successfully!", "email sent", "OK");
            }
            catch (Exception ex)
            {
                // Handle errors
                string StatusLabel = $"Status: Error - {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(StatusLabel, "email not sent", "OK");
            }
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Status: Email cancelled", "email not sent", "OK");
        }
    }


    private void OnSubmitClicked(object sender, EventArgs e)
    {
        // Get the user input
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;
        DateTime date = DatePicker.Date;

        // Trigger the event with the user input
        OnSubmit?.Invoke(email, password, date);


        // Close the popup on the UI thread
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await OnEmailButtonClicked(email, password, date);

            //await Navigation.PopModalAsync();
            await App.Current.MainPage.Navigation.PopModalAsync();
        });
    }
}