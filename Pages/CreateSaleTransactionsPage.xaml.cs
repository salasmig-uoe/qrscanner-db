using QRScanner.Database;
using QRScanner.Services;
using QRScanner.ViewModel;

namespace QRScanner.Pages;

public partial class CreateSaleTransactionsPage : ContentPage
{
    private readonly LocalDbService _dbService;
    private int _editPaymentTransferRecordId;
    private DateTime _editCreateUpdateDate;
    private int _editPaymentItemId;
    private int _capturedItem;

    private string _lastCreated;
    private string _lastTransactionGroup;
    private ArtItem _loaded_item;

    // Popup variables
    private MainViewModel VM;

    // Required to automatically calculate the sale amount
    private decimal _unitPrice = 0.00m;  
    private bool _isQuantityChanging = false;

    public CreateSaleTransactionsPage(LocalDbService dbService, MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _dbService = dbService;

        // Variables for the Popup
        VM = vm;
        BindingContext = VM;

        // Set values manually
        ItemCodeLabel.Text = "000000000";
        ArtistCodeLabel.Text = "Artist0000000";
        TitleLabel.Text = "Title";
        MaterialLabel.Text = "";
        DimensionsLabel.Text = "";

        VM.TotalAmount = 0;
        VM.CardAmount = 0;
        VM.CashAmount = 0;
        VM.DonationAmount = 0;

        VM.DbPrice = 0;
        VM.DbPriceBalance = 0;
        VM.DbAmount = 0;
        VM.DbAmountBalance = 0;

        quantityEntryField.TextChanged += OnQuantityChanged;
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

    public void UpdateParsedScanResults(ArtistItemData data)
    {
        // Updating the Headers
        ItemCodeLabel.Text = data.ItemCodeLabel;
        ArtistCodeLabel.Text = data.ArtistCodeLabel;
        TitleLabel.Text = data.TitleLabel;
        MaterialLabel.Text = data.MaterialLabel;
        DimensionsLabel.Text = data.DimensionsLabel;

        // Updating the payment row
        itemCodeEntryField.Text = data.ItemCodeLabel;
        quantityEntryField.Text = "0"; //Quantity will be 0 by default to force input
        amountEntryField.Text = "0";   //Amount will change automatically when Quantity is updated
        _unitPrice = decimal.Parse(data.PriceLabel);


        // Execute the list update
        Task.Run(async () =>
        {
            // Extract the date from the transaction code
            string formattedDate = string.Empty;
            string transaction_code = transaction_group_EntryLabel.Text;
            if (transaction_group_EntryLabel != null && transaction_group_EntryLabel.Text != "")
            {
                formattedDate = getDateFromCode(transaction_code);
                var items = await _dbService.GetPaymentTransferByCodeAndDate(transaction_code, formattedDate);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    listView.ItemsSource = items;
                    calculateTotals(items);
                });

                string item_code = ItemCodeLabel.Text;
                var art_item = await _dbService.GetByItemCode(item_code);
                if (art_item != null)
                {
                    VM.DbPrice = (decimal)art_item.Price;
                    VM.DbPriceBalance = (decimal)art_item.PriceBalance;
                    VM.DbAmount = (decimal)art_item.Amount;
                    VM.DbAmountBalance = (decimal)art_item.AmountBalance;
                    _loaded_item = art_item;
                }
            }
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
        MainThread.BeginInvokeOnMainThread(() =>
        {
            new_key = str_today + "-" + folio_to_use.ToString("D4");
            transaction_group_EntryLabel.Text = new_key;
            transactionGroupIdEntryField.Text = new_key;
            time_EntryLabel.Text = "(" + transaction_time + ")";
        });
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
        var action = await DisplayActionSheet("Action", "Cancel", null, "Edit", "Delete");
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

                String formattedDate = item.Created.ToString("yyyy-MM-dd");
                var items = await _dbService.GetPaymentTransferByCodeAndDate(item.TransactionCode, formattedDate);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    listView.ItemsSource = items;
                    calculateTotals(items);
                });
                break;
        }
    }

    private async void addItem(string item_code, float quantity, float payment, string ttype,
        string transaction_code, string formattedDate)
    {
        // Add PaymentTransaction
        await _dbService.CreatePaymentItem(new PaymentTransaction
        {
            TransactionCode = transaction_group_EntryLabel.Text,
            ItemCode = item_code,
            Quantity = quantity,
            Amount = payment,
            TransactionType = ttype,
            Created = DateTime.Now,
            Updated = DateTime.Now,
        });
        var items = await _dbService.GetPaymentTransferByCodeAndDate(transaction_code, formattedDate);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Update the listview 
            listView.ItemsSource = items;
            calculateTotals(items);
        });
        
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

    private async void saveSaleTransaction()
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
        // Extract the date from the transaction group date
        string transaction_code = transaction_group_EntryLabel.Text;
        string formattedDate = getDateFromCode(transaction_code);

        addItem(item_code, quantity, payment_float, selectedPaymentType, 
            transaction_code, formattedDate);
        
        updateItemHeader(payment_float, quantity);
    }


    private async void clearButton_Clicked(object sender, EventArgs e)
    {
        transaction_group_EntryLabel.Text = "";
        transactionGroupIdEntryField.Text = "";
        time_EntryLabel.Text = "(--:--)";

        ItemCodeLabel.Text = "";
        ArtistCodeLabel.Text = "";
        TitleLabel.Text = "";
        MaterialLabel.Text = "";
        DimensionsLabel.Text = "";

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
            //Check if there is another record for the same item already in receipt
            String item_code = itemCodeEntryField.Text;
            String transaction_code = transaction_group_EntryLabel.Text;
            var existingTransaction = await _dbService.GetItemsGroupNotDoneAsync(item_code,transaction_code);
            if (existingTransaction.Count > 0)
            {
                string validation_message = "Please edit item or use a different receipt";
                await App.Current.MainPage.DisplayAlert("Item already in receipt ", validation_message, "Ok");
                return;
            }
            int quantity = int.Parse(quantityEntryField.Text);
            // Validating there is enough inventory left
            if (_loaded_item.AmountBalance - quantity < 0)
            {
                string error_message = $"Not enough items to sell ({_loaded_item.AmountBalance})";
                await App.Current.MainPage.DisplayAlert("Please correct the item quantity", error_message, "Ok");
                return;
            }
            saveSaleTransaction();
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
            var items = await _dbService.GetPaymentTransferByCodeAndDate(transaction_code, formattedDate);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                listView.ItemsSource = items;
                calculateTotals(items);
            });
        }
        quantityEntryField.Text = "0";
        amountEntryField.Text = "0";
        string message = " The changes has been saved";
        await App.Current.MainPage.DisplayAlert("Operation completed: ", message, "Ok");
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

    private async Task refresh_item(string item_code)
    {
        var art_item = await _dbService.GetByItemCode(item_code);

        if (art_item != null)
        {
            var artistItemData = new ArtistItemData
            {
                TextboxText = art_item.ItemCode,
                ItemCodeLabel = art_item.ItemCode,
                ArtistCodeLabel = art_item.ArtistCode,
                TitleLabel = art_item.Title,
                MaterialLabel = art_item.WorkType,
                DimensionsLabel = art_item.Size,
                PriceLabel = art_item.Price.ToString("0.00"),
                AmountLabel = art_item.Amount.ToString(),
            };
            UpdateParsedScanResults(artistItemData);
        }
        else
        {
            string message = "The item code does not exist in the database";
            await App.Current.MainPage.DisplayAlert("Error: ", message, "Ok");
        }
    }
    private void analyseContent(String strcode)
    {
        String[] fields = strcode.Split(':');
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Dictionary<string, string> field_key = new Dictionary<string, string>(8);
            String full_text = "";
            string item_code = fields[0].Trim();
            await refresh_item(item_code);
        });
    }

    private async void OnScanQRCodeClicked(object sender, EventArgs e)
    {

        if (transaction_group_EntryLabel is null || transaction_group_EntryLabel.Text is null || transaction_group_EntryLabel.Text == "")
        {
            string message = "This is required before your FIRST scan to group customer sales";
            await App.Current.MainPage.DisplayAlert("You need to click the Recepit icon before Scanning", message, "Ok");

            return;
        }

        // Create the CameraPopupPage
        var scannerPage = new CameraPopupPage();

        // Push the page modally
        await Navigation.PushModalAsync(scannerPage);

        // Await the QR code result from the TaskCompletionSource
        var qrCodeValue = await scannerPage.QRCodeTaskCompletionSource.Task;

        analyseContent(qrCodeValue);
    }

    private async void OnClosingSaleButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    public async Task<string> GetUserInputAsync(string title, string message)
    {
        // Display a prompt and await the user's response
        string result = await Shell.Current.DisplayPromptAsync(title, message);

        // Return the user's input
        return result;
    }

    private async void OnManualQRCodeClicked(object sender, EventArgs e)
    {
        if (transaction_group_EntryLabel is null || transaction_group_EntryLabel.Text is null || transaction_group_EntryLabel.Text == "")
        {
            string message = "This is required before your FIRST scan to group customer sales";
            await App.Current.MainPage.DisplayAlert("You need to click the Recepit icon before Scanning", message, "Ok");

            return;
        }
        // Call the method to display the popup
        string userInput = await GetUserInputAsync("Art Item Code", "Maximum of 7 characters:");

        // Check if the user entered something (not null or empty)
        if (!string.IsNullOrEmpty(userInput))
        {
            string item_code = userInput.Trim().ToUpper();
            await refresh_item(item_code);
            // Do something with the user's input
            await DisplayAlert("Input Received", $"You entered: {userInput}", "OK");
        }
        else
        {
            // Handle the case where the user canceled or didn't enter anything
            await DisplayAlert("No Input", "You didn't enter anything.", "OK");
        }
    }
}