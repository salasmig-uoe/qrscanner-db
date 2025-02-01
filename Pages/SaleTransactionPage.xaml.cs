using CommunityToolkit.Maui.Views;
using QRScanner.Database;
using QRScanner.Services;
using QRScanner.ViewModel;
using QRScanner.Popups;
namespace QRScanner.Pages;

public partial class SaleTransactionPage : ContentPage
{
    private readonly LocalDbService _dbService;
    private int _editPaymentTransferRecordId;
    private DateTime _editCreateUpdateDate;
    private int _editPaymentItemId;
    private int _capturedItem;

    private string _lastCreated;
    private string _lastTransactionGroup;


    // Popup variables
    private MainViewModel VM;
    private PopupResult _result;


    public void UpdateParsedScanResults(ArtistItemData data)
    {
        // Updating the Headers
        ItemCodeLabel.Text = data.ItemCodeLabel;
        ArtistCodeLabel.Text = data.ArtistCodeLabel;
        TitleLabel.Text = data.TitleLabel;
        MaterialLabel.Text = data.MaterialLabel;
        DimensionsLabel.Text = data.DimensionsLabel;
        PriceLabel.Text = data.PriceLabel;
        // Updating the payment row
        itemCodeEntryField.Text = data.ItemCodeLabel;
        quantityEntryField.Text = "1";
        amountEntryField.Text = data.PriceLabel;

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
            }
        });
    }

    public SaleTransactionPage(LocalDbService dbService, MainViewModel vm, PopupResult result)
    {
        InitializeComponent();
        BindingContext = vm;
        _dbService = dbService;

        // Variables for the Popup
        VM = vm;
        _result = result;
        BindingContext = VM;

        // Set values manually
        ItemCodeLabel.Text = "000000000";
        ArtistCodeLabel.Text = "Artist0000000";
        TitleLabel.Text = "Title";
        MaterialLabel.Text = "";
        DimensionsLabel.Text = "";
        PriceLabel.Text = "0";
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
                totalCard += (decimal) item.Amount; // Adjust the property names as per your model
            if (item.TransactionType == "Cash")
                totalCash += (decimal) item.Amount;
            if (item.TransactionType == "Donation")
                totalDonation += (decimal) item.Amount;
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

        // Add PaymentTransaction
        await _dbService.CreatePaymentItem(new PaymentTransaction
        {
            TransactionCode = transaction_group_EntryLabel.Text,
            ItemCode = item_code,
            Quantity = quantity,
            Amount = payment_float,
            TransactionType = selectedPaymentType,
            Created = DateTime.Now,
            Updated = DateTime.Now,
        });

        // Extract the date from the transaction group date
        string transaction_code = transaction_group_EntryLabel.Text;
        string formattedDate = getDateFromCode(transaction_code);


        var items = await _dbService.GetPaymentTransferByCodeAndDate(transaction_code, formattedDate);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            listView.ItemsSource = items;
            calculateTotals(items);
        });
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
            var items = await _dbService.GetPaymentTransferByCodeAndDate(transaction_code, formattedDate);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                listView.ItemsSource = items;
                calculateTotals(items);
            });
        }
        quantityEntryField.Text = "1";
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

    private async void OnScanButtonClicked(object sender, EventArgs e)
    {

        if (transaction_group_EntryLabel is null || transaction_group_EntryLabel.Text is null || transaction_group_EntryLabel.Text == "")
        {
            string message = "You need to generate a transaction group before Scanning";
            await App.Current.MainPage.DisplayAlert("Error: ", message, "Ok");

            return;
        }
        var popupPage = new PopupPage(VM, _result);
        var result = await this.ShowPopupAsync(popupPage);
        if (result != null)
        {
            PopupResult res = (PopupResult)result;
            VM.BarcodeLabelText = res.ReturnData;
            VM.Text = res.ReturnData;
        }
    }

    private void analyseContent(String strcode)
    {
        String[] fields = strcode.Split(':');
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Dictionary<string, string> field_key = new Dictionary<string, string>(8);
            String full_text = "";
            for (int i = 0; i < fields.Length; i++)
            {
                String field_value = fields[i].Trim();
                switch (i)
                {
                    case 0:
                        field_key.Add("artist_code", field_value);
                        break;
                    case 1:
                        field_key.Add("title", field_value);
                        full_text = "Title:   " + field_value + "    ";
                        break;
                    case 2:
                        field_key.Add("work_type", field_value);
                        full_text += "Media:    " + field_value + "     ";
                        break;
                    case 3:
                        field_key.Add("size", field_value);
                        full_text += "Dimensions:    " + field_value + " (cm)";
                        break;
                    case 4:
                        field_key.Add("price", field_value);
                        break;
                    case 5:
                        field_key.Add("amount", field_value);
                        break;
                    case 6:
                        field_key.Add("item_code", field_value);
                        break;
                    case 7:
                        field_key.Add("qr_id", field_value);
                        break;
                }
            }

            var artistItemData = new ArtistItemData
            {
                TextboxText = field_key["item_code"],
                ItemCodeLabel = field_key["item_code"],
                ArtistCodeLabel = field_key["artist_code"],
                TitleLabel = field_key["title"],
                MaterialLabel = field_key["work_type"],
                DimensionsLabel = field_key["size"],
                PriceLabel = field_key["price"],
            };

            UpdateParsedScanResults(artistItemData);
        });
    }

    private async void OnScanQRCodeClicked(object sender, EventArgs e)
    {

        if (transaction_group_EntryLabel is null || transaction_group_EntryLabel.Text is null || transaction_group_EntryLabel.Text == "")
        {
            string message = "You need to generate a transaction group before Scanning";
            await App.Current.MainPage.DisplayAlert("Error: ", message, "Ok");

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
}