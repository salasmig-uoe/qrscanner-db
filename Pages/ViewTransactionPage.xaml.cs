using QRScanner.Database;
using QRScanner.Services;
using QRScanner.ViewModel;
using QRScanner.Popups;
namespace QRScanner.Pages;

public partial class ViewTransactionPage : ContentPage
{
    private readonly LocalDbService _dbService;
    private int _editPaymentTransferRecordId;
    private DateTime _editCreateUpdateDate;
    private int _editPaymentItemId;
    private int _capturedItem;

    private string _lastCreated;
    private string _lastTransactionGroup;
    private DateTime _queryDate;

    // Popup variables
    private MainViewModel VM;
    private PopupResult _result;
    public ViewTransactionPage(LocalDbService dbService, MainViewModel vm)
    {
        InitializeComponent();

        // Trigger the method when the DatePicker is initialized
        OnDateInitialized(datePicker.Date);

        BindingContext = vm;
        _dbService = dbService;

        // Variables for the Popup
        VM = vm;

        BindingContext = VM;
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        _queryDate = e.NewDate;
        Task.Run(async () =>
        {
            string formattedDate = getDateFromCode(_queryDate.ToString("ddMMyy"));
            var items = await _dbService.GetPaymentTransferByDate(formattedDate);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                listView.ItemsSource = items;
                calculateTotals(items);
            });
        });
    }

    private void OnDateInitialized(DateTime initialDate)
    {
        Task.Run(async () =>
        {
            DateTime _queryDate = initialDate;
            string formattedDate = getDateFromCode(_queryDate.ToString("ddMMyy"));
            await Task.Delay(TimeSpan.FromSeconds(1));
            var items = await _dbService.GetPaymentTransferByDate(formattedDate);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                listView.ItemsSource = items;
                calculateTotals(items);
            });
        });

    }
    private string getDateFromCode(string str_code)
    {
        string datePart = str_code.Substring(0, 6); // Extract "230125"
        DateTime date = DateTime.ParseExact(datePart, "ddMMyy", null);
        string formattedDate = date.ToString("yyyy-MM-dd");
        return formattedDate;
    }

    private void calculateTotals(List<PaymentTransaction> items)
    {
        decimal totalCard = 0;
        decimal totalCash = 0;
        decimal totalDonation = 0;
        decimal total = 0;
        // Iterate over items to calculate totals
        foreach (var item in items)
        {
            if (item.TransactionType == "Card")
                // Assuming item has Card, Cash or Donation Amount properties
                totalCard += (decimal)item.Amount; // Adjust the property names as per your model
            if (item.TransactionType == "Cash")
                totalCash += (decimal)item.Amount;
            if (item.TransactionType == "Donation")
                totalDonation += (decimal)item.Amount;
        }
        total = totalCard + totalCash + totalDonation;

        VM.CardAmount = totalCard;
        VM.CashAmount = totalCash;
        VM.DonationAmount = totalDonation;
        VM.TotalAmount = total;
    }

    private async Task UpdateDatabaseAndUIAsync()
    {
        String str_today = DateTime.Now.ToString("ddMMyy");
        String new_key = "";


        var last_transaction = await _dbService.GetLastTransactionAsync(str_today);
        int folio_to_use = 0;
        string transaction_time;

        if (last_transaction is null)
        {
            await _dbService.CreateLastTransaction(new LastTransactions
            {
                BaseCode = str_today,
                LastFolio = 1,
                Updated = DateTime.Now,
            });
            folio_to_use = 1;
        }
        else
        {
            folio_to_use = last_transaction.LastFolio + 1;
            await _dbService.UpdateLastTransaction(new LastTransactions
            {
                Id = last_transaction.Id,
                BaseCode = last_transaction.BaseCode,
                LastFolio = folio_to_use,
                Updated = DateTime.Now,
            });
        }
        transaction_time = DateTime.Now.ToString("HH:mm");
    }
    public void generateNextTransactionGroupButton_Clicked(object sender, EventArgs e)
    {
        Task.Run(async () => await UpdateDatabaseAndUIAsync());
    }

    private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
    {

    }
    private async void OnGoBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}