using QRScanner.Database;
using QRScanner.Services;
using QRScanner.ViewModel;
namespace QRScanner.Pages;

public partial class EditSaleTransactionsPage : ContentPage
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
    
    public EditSaleTransactionsPage(LocalDbService dbService, MainViewModel vm)
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


    // Method to update the Picker based on a string value
    public void UpdateTransactionTypePicker(string newValue)
    {
        if (transactionTypePicker.ItemsSource is IList<string> items && items.Contains(newValue))
        {
            transactionTypePicker.SelectedItem = newValue;
        }
    }
    private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        var item = (PaymentTransaction)e.Item;
        var action = await DisplayActionSheet("Action", "Cancel", null, "Edit", "Duplicate", "Delete");
        String formattedDate;

        switch (action)
        {
            case "Edit":
                _editPaymentTransferRecordId = item.PaymentId;
                paymentIdEntryField.Text = item.PaymentId.ToString();
                itemCodeEntryField.Text = item.ItemCode;
                quantityEntryField.Text = item.Quantity.ToString();
                UpdateTransactionTypePicker(item.TransactionType);
                amountEntryField.Text = item.Amount.ToString();
                _editCreateUpdateDate = DateTime.Now;
                break;
            case "Delete":
                await _dbService.DeletePaymentTransaction(item);
                formattedDate = item.Created.ToString("yyyy-MM-dd");
                var items = await _dbService.GetPaymentTransferByDate(formattedDate);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    listView.ItemsSource = items;
                    calculateTotals(items);
                });
                break;
            case "Duplicate":
                await _dbService.CreatePaymentItem(new PaymentTransaction
                {
                    TransactionCode = item.TransactionCode,
                    ItemCode = item.ItemCode,
                    Quantity = item.Quantity,
                    Amount = 0,
                    TransactionType = item.TransactionType,
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                });
                formattedDate = item.Created.ToString("yyyy-MM-dd");
                items = await _dbService.GetPaymentTransferByDate(formattedDate);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    listView.ItemsSource = items;
                    calculateTotals(items);
                });
                break;
        }
    }

    private async void detailSaveButton_Clicked(object sender, EventArgs e)
    {
        String payment_str = amountEntryField.Text;
        String item_code = itemCodeEntryField.Text;
        float payment_float = float.Parse(payment_str);
        String selectedPaymentType = "Cash";
        int quantity = int.Parse(quantityEntryField.Text);
        if (transactionTypePicker.SelectedIndex != -1)
        {
            selectedPaymentType = transactionTypePicker.SelectedItem.ToString();
        }

        _queryDate = datePicker.Date;
        // Extract the date from the transaction group date
        string formattedDate = getDateFromCode(_queryDate.ToString("ddMMyy"));
        var items = await _dbService.GetPaymentTransferByDate(formattedDate);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            listView.ItemsSource = items;
            calculateTotals(items);
        });

    }

    private async void clearButton_Clicked(object sender, EventArgs e)
    {
        paymentIdEntryField.Text = "";
        itemCodeEntryField.Text = "";
        quantityEntryField.Text = "";
        amountEntryField.Text = "";
        transactionTypePicker.SelectedIndex = -1;
        _editPaymentTransferRecordId = 0;
    }

    private async void applyButton_Clicked(object sender, EventArgs e)
    {
        if (itemCodeEntryField.Text == "")
        {
            string validation_message = "You need to enter an item code before saving";
            await App.Current.MainPage.DisplayAlert("Error: ", validation_message, "Ok");
            return;
        }
        if (amountEntryField.Text == "0" || amountEntryField.Text == "")
        {
            string validation_message = "You need to enter an amount before saving";
            await App.Current.MainPage.DisplayAlert("Error: ", validation_message, "Ok");
            return;
        }
        if (quantityEntryField.Text == "0" || quantityEntryField.Text == "")
        {
            string validation_message = "You need to enter a quantity before saving";
            await App.Current.MainPage.DisplayAlert("Error: ", validation_message, "Ok");
            return;
        }

        if (_editPaymentTransferRecordId == 0)
        {
            detailSaveButton_Clicked(sender, e);
        }
        else
        {
            String payment_str = amountEntryField.Text;
            String item_code = itemCodeEntryField.Text;
            float payment_float = float.Parse(payment_str);
            String selectedPaymentType = "Cash";
            int quantity = int.Parse(quantityEntryField.Text);
            if (transactionTypePicker.SelectedIndex != -1)
            {
                selectedPaymentType = transactionTypePicker.SelectedItem.ToString();
            }

            var existingTransaction = await _dbService.GetPaymentItemByIdAsync(_editPaymentTransferRecordId);

            // Add PaymentTransaction
            await _dbService.UpdatePaymentItem(new PaymentTransaction
            {
                PaymentId = _editPaymentTransferRecordId,
                TransactionCode = existingTransaction.TransactionCode,
                ItemCode = item_code,
                Quantity = quantity,
                Amount = payment_float,
                TransactionType = selectedPaymentType,
                Updated = DateTime.Now,
                Created = existingTransaction.Created,
            });
            _editPaymentItemId = 0;

            String formattedDate = existingTransaction.Created.ToString("yyyy-MM-dd");
            string transaction_code = existingTransaction.TransactionCode;
            var items = await _dbService.GetPaymentTransferByDate(formattedDate);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                listView.ItemsSource = items;
                calculateTotals(items);
            });
            clearButton_Clicked(sender, e);
        }
        string message = " The changes has been saved";
        await App.Current.MainPage.DisplayAlert("Operation completed: ", message, "Ok");
        clearButton_Clicked(sender, e);
    }


    private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (transactionTypePicker is not null)
        {
            if (transactionTypePicker.SelectedIndex != -1)
            {
                string selectedPaymentType = transactionTypePicker.SelectedItem.ToString();
            }
        }
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        if (double.TryParse(amountEntryField.Text, out double amount))
        {
            amountEntryField.Text = amount.ToString("F2");
        }
    }
    private async void OnGoBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}