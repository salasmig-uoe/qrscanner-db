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
    private ArtItem _loaded_item;
    private DateTime _queryDate;

    // Popup variables
    private MainViewModel VM;

    // Required to automatically calculate the sale amount
    private decimal _unitPrice = 0.00m;
    private bool _isQuantityChanging = false;
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

        quantityEntryField.TextChanged += OnQuantityChanged;
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

    private void OnQuantityChanged(object sender, TextChangedEventArgs e)
    {
        if (_isQuantityChanging) return;

        try
        {
            _isQuantityChanging = true;

            // Format the quantity to 2 decimal places
            if (decimal.TryParse(e.NewTextValue, out decimal quantity))
            {
                quantityEntryField.Text = quantity.ToString();

                // Calculate and update the amount
                decimal amount = quantity * _unitPrice;
                amountEntryField.Text = amount.ToString("C");
            }
            else if (string.IsNullOrEmpty(e.NewTextValue))
            {
                amountEntryField.Text = string.Empty;
            }
        }
        finally
        {
            _isQuantityChanging = false;
        }
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
        //TODO: Remove "Duplicate" process
        var action = await DisplayActionSheet("Action", "Cancel", null, "Edit", "Delete");
        String formattedDate;
        // Get the full item loaded
        _loaded_item = await _dbService.GetByItemCode(item.ItemCode);
        _unitPrice = (decimal)_loaded_item.Price;
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
                float quantity = -item.Quantity; // Towards Quantity
                float payment_float = -item.Amount; // Towards Price

                updateItemHeader(payment_float, quantity);

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

    private async void updateItemHeader(float payment_float, float quantity)
    {
        await _dbService.Update(new ArtItem
        {
            Id = _loaded_item.Id,
            Title = _loaded_item.Title,
            WorkType = _loaded_item.WorkType,
            Size = _loaded_item.Size,
            ArtistCode = _loaded_item.ArtistCode,
            ArtistName = _loaded_item.ArtistName,
            Price = _loaded_item.Price,
            Amount = _loaded_item.Amount,
            PriceBalance = _loaded_item.PriceBalance - payment_float,
            AmountBalance = _loaded_item.AmountBalance - quantity,
            ItemStatus = _loaded_item.ItemStatus,
            ItemCode = _loaded_item.ItemCode,
            Created = _loaded_item.Created,
            Updated = DateTime.Now
        });

        var updated_art_item = await _dbService.GetById(_loaded_item.Id);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Update the ArtItem balances in the header
            VM.DbPrice = (decimal)updated_art_item.Price;
            VM.DbPriceBalance = (decimal)updated_art_item.PriceBalance;
            VM.DbAmount = (decimal)updated_art_item.Amount;
            VM.DbAmountBalance = (decimal)updated_art_item.AmountBalance;
            _loaded_item = updated_art_item;
        });
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
            float payment_float = float.Parse(payment_str.Replace("£",""));
            String selectedPaymentType = "Cash";
            int quantity = int.Parse(quantityEntryField.Text);
            if (transactionTypePicker.SelectedIndex != -1)
            {
                selectedPaymentType = transactionTypePicker.SelectedItem.ToString();
            }

            var existingTransaction = await _dbService.GetPaymentItemByIdAsync(_editPaymentTransferRecordId);
            
            _loaded_item = await _dbService.GetByItemCode(item_code);

            float amount_difference = -(existingTransaction.Amount - payment_float); // Towards price
            float quantity_difference = -(existingTransaction.Quantity - quantity); // Towards quantity

            // Validating there is enough inventory left
            if (_loaded_item.AmountBalance - quantity_difference < 0)
            {
                string error_message = $"Not enough items to sell ({_loaded_item.AmountBalance})";
                await App.Current.MainPage.DisplayAlert("Please correct the item quantity", error_message, "Ok");
                return;
            }
 
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

            updateItemHeader(amount_difference, quantity_difference);

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