using CommunityToolkit.Maui.Views;
using QRScanner.Database;
using QRScanner.Services;
using QRScanner.ViewModel;
using QRScanner.Popups;
namespace QRScanner.Pages;

public partial class DetailViewPage : ContentPage
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

    public DetailViewPage(LocalDbService dbService, MainViewModel vm, PopupResult result)
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

        MessagingCenter.Subscribe<PopupPage, ArtistItemData>(this, "UpdateControls", (sender, data) =>
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
                if (transaction_group_EntryLabel != null && transaction_group_EntryLabel.Text != "")
                {
                    string datePart = transaction_group_EntryLabel.Text.Substring(0, 6); // Extract "230125"
                    DateTime date = DateTime.ParseExact(datePart, "ddMMyy", null);
                    formattedDate = date.ToString("yyyy-MM-dd");

                    
                    var items = await _dbService.GetPaymentTransferByDate(formattedDate);
                    await Device.InvokeOnMainThreadAsync(() =>
                    {
                        listView.ItemsSource = items;
                        calculateTotals(items);
                    });                    
                }
            });
        });
    }

    private void calculateTotals(List<PaymentTransaction> items)
    {
        float totalCard = 0;
        float totalCash = 0;
        float total = 0;
        // Iterate over items to calculate totals
        foreach (var item in items)
        {
            if (item.TransactionType == "Card")
                // Assuming item has CardAmount and CashAmount properties
                totalCard += item.Amount; // Adjust the property names as per your model
            if (item.TransactionType == "Cash")
                totalCash += item.Amount;
        }
        total = totalCard + totalCash;

        CardAmountLabel.Text = totalCard.ToString();
        CashAmountLabel.Text = totalCash.ToString();
        TotalAmountLabel.Text = total.ToString();
    }
    
    public void generateNextTransactionGroupButton_Clicked(object sender, EventArgs e)
    {
        String str_today = DateTime.Now.ToString("ddMMyy");
        String new_key = "";
        String old_key = transaction_group_EntryLabel.Text; 
        if (old_key is null || old_key.Trim() == "")
            new_key = str_today + "-00001";
        else
            new_key = str_today + "-00002";
        transaction_group_EntryLabel.Text = new_key;
        transactionGroupIdEntryField.Text = new_key;
    }


    private async void OnScanButtonClicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync(new PopupPage(VM, _result));
        if (result != null)
        {
            PopupResult res = (PopupResult)result;
            VM.BarcodeLabelText = res.ReturnData;
            VM.Text = res.ReturnData;
        }
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
                UpdateTransactionTypePicker(item.TransactionType);
                amountEntryField.Text = item.Amount.ToString();
                _editCreateUpdateDate = DateTime.Now;
                break;
            case "Delete":
                await _dbService.DeletePaymentTransaction(item);
                
                String formattedDate = item.Created.ToString("yyyy-MM-dd");
                var items = await _dbService.GetPaymentTransferByDate(formattedDate);
                Device.BeginInvokeOnMainThread(() =>
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
        string formattedDate = string.Empty;
        string datePart = transaction_group_EntryLabel.Text.Substring(0, 6);
        DateTime date = DateTime.ParseExact(datePart, "ddMMyy", null);
        formattedDate = date.ToString("yyyy-MM-dd");
        var items = await _dbService.GetPaymentTransferByDate(formattedDate);
        Device.BeginInvokeOnMainThread(() =>
        {
            listView.ItemsSource = items;
            calculateTotals(items);
        });
    }

    private async void savePaymentTransactionButton_Clicked(object sender, EventArgs e)
    {
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
            var items = await _dbService.GetPaymentTransferByDate(formattedDate);
            Device.BeginInvokeOnMainThread(() =>
            {
                listView.ItemsSource = items;
                calculateTotals(items);
            });
        }
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
}